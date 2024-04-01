using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using MessagePush.Common;
using MessagePush.Commons;
using MessagePush.DeviceInfo;
using MessagePush.Entities.Es;
using MessagePush.MessagePush.Dtos;
using MessagePush.MessagePush.Provider;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nest;
using Newtonsoft.Json;
using Volo.Abp;
using Volo.Abp.Validation;

namespace MessagePush.MessagePush;

[RemoteService(false)]
public class MessagePushAppService : MessagePushBaseService, IMessagePushAppService
{
    private readonly IMessagePushProvider _messagePushProvider;
    private readonly INESTRepository<UnreadMessageIndex, string> _unreadMessageIndexRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MessagePushAppService(IMessagePushProvider messagePushProvider,
        INESTRepository<UnreadMessageIndex, string> unreadMessageIndexRepository,
        IHttpContextAccessor httpContextAccessor)
    {
        _messagePushProvider = messagePushProvider;
        _unreadMessageIndexRepository = unreadMessageIndexRepository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task PushMessageAsync(MessagePushDto input)
    {
        var appId = GetAppId();
        var userDevices = await _messagePushProvider.GetUserDevicesAsync(input.UserIds, appId);
        userDevices = userDevices?.Where(t =>
            !t.AppStatus.Equals(AppStatus.Foreground.ToString(), StringComparison.OrdinalIgnoreCase)).ToList();
        if (userDevices.IsNullOrEmpty()) return;

        var userIds = userDevices.Select(t => t.UserId).ToList();
        var unreadMessageInfos = await UpdateUnreadCount(userIds);
        
        var handleAndroidDevicesTask = HandleAndroidDevicesAsync(userDevices, input);
        var handleAppleDevicesTask = HandleAppleDevicesAsync(userDevices, unreadMessageInfos, input);
        var handleExtensionDevicesTask = HandleExtensionDevicesAsync(userDevices, unreadMessageInfos, input);

        await Task.WhenAll(handleAndroidDevicesTask, handleAppleDevicesTask, handleExtensionDevicesTask);
    }

    public async Task ClearMessageAsync(ClearMessageDto input)
    {
        var userDevice = await _messagePushProvider.GetUserDeviceAsync(input.UserId, input.DeviceId, input.AppId);
        await _messagePushProvider.PushAsync(userDevice.RegistrationToken, string.Empty,
            CommonConstant.DefaultTitle,
            CommonConstant.DefaultContent, input.Data, badge: 0);
    }

    private async Task<List<UnreadMessageIndex>> GetUnreadInfosAsync(List<string> userIds)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<UnreadMessageIndex>, QueryContainer>>()
        {
            descriptor => descriptor.Terms(i => i.Field(f => f.Id).Terms(userIds))
        };

        QueryContainer Filter(QueryContainerDescriptor<UnreadMessageIndex> f) => f.Bool(b => b.Must(mustQuery));
        var result = await _unreadMessageIndexRepository.GetListAsync(Filter);

        return result.Item2;
    }

    private async Task<List<UnreadMessageIndex>> UpdateUnreadCount(List<string> userIds)
    {
        try
        {
            var unreadInfos = await GetUnreadInfosAsync(userIds);

            foreach (var unreadInfo in unreadInfos)
            {
                if (unreadInfo.UnreadMessageInfos.IsNullOrEmpty())
                {
                    unreadInfo.UnreadMessageInfos = new List<UnreadMessageInfo>
                    {
                        new UnreadMessageInfo
                        {
                            MessageType = MessageType.RelationOne.ToString(),
                            UnreadCount = 1
                        }
                    };

                    continue;
                }

                var imMessage = unreadInfo.UnreadMessageInfos.FirstOrDefault(t =>
                    t.MessageType.Equals(MessageType.RelationOne.ToString(), StringComparison.OrdinalIgnoreCase));
                if (imMessage == null)
                {
                    unreadInfo.UnreadMessageInfos.Add(new UnreadMessageInfo()
                    {
                        MessageType = MessageType.RelationOne.ToString(),
                        UnreadCount = 1
                    });
                }

                imMessage.UnreadCount++;
            }

            if (!unreadInfos.IsNullOrEmpty())
            {
                await _unreadMessageIndexRepository.BulkAddOrUpdateAsync(unreadInfos);
            }

            return unreadInfos;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "param: {data}", JsonConvert.SerializeObject(userIds));
            return new List<UnreadMessageIndex>();
        }
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

        var androidTokens = androidDevices.Select(t => t.RegistrationToken).ToList();
        await _messagePushProvider.BulkPushAsync(androidTokens, input.Icon, input.Title, input.Content, input.Data);
    }

    private async Task HandleAppleDevicesAsync(List<UserDeviceIndex> userDevices,
        List<UnreadMessageIndex> unreadMessageInfos, MessagePushDto input)
    {
        // ios users
        var iosTokenInfos = userDevices
            .Where(t => t.DeviceInfo.DeviceType.Equals(DeviceType.IOS.ToString(), StringComparison.OrdinalIgnoreCase))
            .Select(t => new { t.UserId, t.RegistrationToken }).ToList();

        var pushTasks = iosTokenInfos.Select(tokenInfo =>
        {
            var unreadMessage = unreadMessageInfos.FirstOrDefault(t => t.UserId == tokenInfo.UserId)
                ?.UnreadMessageInfos;
            var badge = UnreadMessageHelper.GetUnreadCount(unreadMessage);

            return _messagePushProvider.PushAsync(tokenInfo.RegistrationToken, input.Icon, input.Title, input.Content,
                input.Data,
                badge);
        });

        await Task.WhenAll(pushTasks);
    }

    private async Task HandleExtensionDevicesAsync(List<UserDeviceIndex> userDevices,
        List<UnreadMessageIndex> unreadMessageInfos, MessagePushDto input)
    {
        var extensionDevices = userDevices
            .Where(t => t.DeviceInfo.DeviceType.Equals(DeviceType.Extension.ToString(),
                StringComparison.OrdinalIgnoreCase))
            .Select(t => new { t.UserId, t.RegistrationToken }).ToList();

        var pushTasks = extensionDevices.Select(tokenInfo =>
        {
            var unreadMessage = unreadMessageInfos.FirstOrDefault(t => t.UserId == tokenInfo.UserId)
                ?.UnreadMessageInfos;
            var badge = UnreadMessageHelper.GetUnreadCount(unreadMessage);

            Logger.LogInformation("push to extension, count: {count}", extensionDevices.Count);
            return _messagePushProvider.PushAsync(tokenInfo.RegistrationToken, input.Icon, input.Title, input.Content,
                input.Data,
                badge);
        });

        await Task.WhenAll(pushTasks);
    }
}