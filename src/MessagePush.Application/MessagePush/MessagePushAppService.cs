using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessagePush.Common;
using MessagePush.Commons;
using MessagePush.DeviceInfo;
using MessagePush.Entities.Es;
using MessagePush.Entities.Redis;
using MessagePush.MessagePush.Dtos;
using MessagePush.MessagePush.Provider;
using MessagePush.Options;
using MessagePush.Redis;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using Newtonsoft.Json;
using Volo.Abp;
using Volo.Abp.Validation;

namespace MessagePush.MessagePush;

[RemoteService(false)]
public class MessagePushAppService : MessagePushBaseService, IMessagePushAppService
{
    private readonly IMessagePushProvider _messagePushProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly MessagePushOptions _messagePushOptions;
    private readonly RedisClient _redisClient;

    public MessagePushAppService(IMessagePushProvider messagePushProvider,
        IHttpContextAccessor httpContextAccessor, IOptionsSnapshot<MessagePushOptions> messagePushOptions, RedisClient redisClient)
    {
        _messagePushProvider = messagePushProvider;
        _httpContextAccessor = httpContextAccessor;
        _messagePushOptions = messagePushOptions.Value;
        _redisClient = redisClient;
    }

    public async Task PushMessageAsync(MessagePushDto input)
    {
        var appId = GetAppId();
        var userDevices = await _messagePushProvider.GetUserDevicesAsync(input.UserIds, appId);
        userDevices = userDevices?.Where(t =>
            !t.AppStatus.Equals(AppStatus.Foreground.ToString(), StringComparison.OrdinalIgnoreCase) &&
            t.ModificationTime > DateTime.Now.SubtractDays(_messagePushOptions.ExpiredDeviceInfoFromDays)).ToList();
        if (userDevices.IsNullOrEmpty()) return;

        var userIds = userDevices.Select(t => t.UserId).Distinct().ToList();
        // var unreadMessageInfos = await UpdateUnreadCount(userIds);
        var unreadMessages = await UpdateUnreadCountAsync(userIds);

        var handleAndroidDevicesTask = HandleAndroidDevicesAsync(userDevices, input);
        var handleAppleDevicesTask = HandleAppleDevicesAsync(userDevices, unreadMessages, input);
        var handleExtensionDevicesTask = HandleExtensionDevicesAsync(userDevices, unreadMessages, input);
        
        await Task.WhenAll(handleAndroidDevicesTask, handleAppleDevicesTask, handleExtensionDevicesTask);
    }

    public async Task ClearMessageAsync(ClearMessageDto input)
    {
        var userDevice = await _messagePushProvider.GetUserDeviceAsync(input.UserId, input.DeviceId, input.AppId);
        await _messagePushProvider.PushAsync(userDevice.Id, userDevice.RegistrationToken, string.Empty,
            CommonConstant.DefaultTitle,
            CommonConstant.DefaultContent, input.Data, badge: 0);
    }

    private async Task<List<UnreadMessage>> GetUnreadMessagesAsync(List<string> userIds)
    {
        List<UnreadMessage> unreadMessages = new List<UnreadMessage>();

        List<Task> tasks = new List<Task>();

        if (userIds != null && userIds.Any())
        {
            foreach (var userId in userIds)
            {
                tasks.Add(Task.Run(() =>
                {
                    var unreadMessage = new UnreadMessage()
                    {
                        UserId = userId,
                        AppId = "PortKey",
                        MessageType = MessageType.RelationOne.ToString()
                    };
                    var value = _redisClient.IncrementAndGet(unreadMessage.GetKey());
                    unreadMessage.UnreadCount = value;
                    unreadMessages.Add(unreadMessage);
                }));
            }
        }

        await Task.WhenAll(tasks);
        return unreadMessages;
    }

    private async Task<List<UnreadMessage>> UpdateUnreadCountAsync(List<string> userIds)
    {
        var unreadMessagesAsync = await GetUnreadMessagesAsync(userIds);
        if (unreadMessagesAsync != null && unreadMessagesAsync.Any())
        {
            foreach (var unreadMessage in unreadMessagesAsync)
            {
                var json = JsonConvert.SerializeObject(unreadMessage);
                Logger.LogInformation("unreadMessage: {json}", json);
            }
        }

        return unreadMessagesAsync;
    }

    private string GetAppId()
    {
        var appId = _httpContextAccessor.HttpContext?.Request.Headers[CommonConstant.AppIdKeyName].ToString();
        if (appId.IsNullOrEmpty())
        {
            throw new AbpValidationException("appId can not be null");
        }

        return appId;
    }

    private async Task HandleAndroidDevicesAsync(List<UserDeviceIndex> userDevices, MessagePushDto input)
    {
        // android user
        var androidDevices = userDevices.Where(t =>
            t.DeviceInfo.DeviceType.Equals(DeviceType.Android.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
        
        await _messagePushProvider.BulkPushAsync(androidDevices, input.Icon, input.Title, input.Content, input.Data);
    }

    private async Task HandleAppleDevicesAsync(List<UserDeviceIndex> userDevices,
        List<UnreadMessage> unreadMessages, MessagePushDto input)
    {
        // ios users
        var iosDevices = userDevices
            .Where(t => t.DeviceInfo.DeviceType.Equals(DeviceType.IOS.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();

        await _messagePushProvider.SendAllAsync(iosDevices, input.Icon, input.Title, input.Content,
            input.Data, unreadMessages);
    }

    private async Task HandleExtensionDevicesAsync(List<UserDeviceIndex> userDevices,
        List<UnreadMessage> unreadMessages, MessagePushDto input)
    {
        var extensionDevices = userDevices
            .Where(t => t.DeviceInfo.DeviceType.Equals(DeviceType.Extension.ToString(),
                StringComparison.OrdinalIgnoreCase))
            .Select(t => new { t.Id, t.UserId, t.RegistrationToken }).ToList();

        var pushTasks = extensionDevices.Select(tokenInfo =>
        {
            var unreadMessage = unreadMessages.FirstOrDefault(t => t.UserId == tokenInfo.UserId);
            var badge = UnreadMessageHelper.GetUnreadCount(unreadMessage);

            Logger.LogInformation("push to extension, count: {count}", extensionDevices.Count);
            return _messagePushProvider.PushAsync(tokenInfo.Id, tokenInfo.RegistrationToken, input.Icon, input.Title, input.Content,
                input.Data,
                badge);
        });

        await Task.WhenAll(pushTasks);
    }
}