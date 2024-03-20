using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using FirebaseAdmin.Messaging;
using MessagePush.Commons;
using MessagePush.Entities.Es;
using Microsoft.Extensions.Logging;
using Nest;
using Newtonsoft.Json;
using Volo.Abp.DependencyInjection;

namespace MessagePush.MessagePush.Provider;

public interface IMessagePushProvider
{
    Task<List<UserDeviceIndex>> GetUserDevicesAsync(List<string> userIds, string appId);
    Task<UserDeviceIndex> GetUserDeviceAsync(string userId, string deviceId, string appId);

    Task BulkPushAsync(List<string> tokens, string icon, string title, string content,
        Dictionary<string, string> data, int badge = 1);

    Task PushAsync(string token, string icon, string title, string content,
        Dictionary<string, string> data, int badge = 1);
}

public class MessagePushProvider : IMessagePushProvider, ISingletonDependency
{
    private readonly INESTRepository<UserDeviceIndex, string> _userDeviceRepository;
    private readonly ILogger<MessagePushProvider> _logger;

    public MessagePushProvider(INESTRepository<UserDeviceIndex, string> userDeviceRepository,
        ILogger<MessagePushProvider> logger)
    {
        _userDeviceRepository = userDeviceRepository;
        _logger = logger;
    }

    public async Task<List<UserDeviceIndex>> GetUserDevicesAsync(List<string> userIds, string appId)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<UserDeviceIndex>, QueryContainer>>() { };
        mustQuery.Add(q => q.Terms(i => i.Field(f => f.UserId).Terms(userIds)));
        //mustQuery.Add(q => q.Term(i => i.Field(f => f.AppId).Value(appId)));

        QueryContainer Filter(QueryContainerDescriptor<UserDeviceIndex> f) => f.Bool(b => b.Must(mustQuery));
        IPromise<IList<ISort>> Sort(SortDescriptor<UserDeviceIndex> s) => s.Descending(t => t.ModificationTime);

        var (totalCount, userDeviceIndices) = await _userDeviceRepository.GetSortListAsync(Filter, sortFunc: Sort);
        return userDeviceIndices;
    }

    public async Task<UserDeviceIndex> GetUserDeviceAsync(string userId, string deviceId, string appId)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<UserDeviceIndex>, QueryContainer>>() { };
        mustQuery.Add(q => q.Term(i => i.Field(f => f.UserId).Value(userId)));
        mustQuery.Add(q => q.Term(i => i.Field(f => f.DeviceId).Value(deviceId)));
        //mustQuery.Add(q => q.Term(i => i.Field(f => f.AppId).Value(appId)));

        QueryContainer Filter(QueryContainerDescriptor<UserDeviceIndex> f) => f.Bool(b => b.Must(mustQuery));
        return await _userDeviceRepository.GetAsync(Filter);
    }

    public async Task BulkPushAsync(List<string> tokens, string icon, string title, string content,
        Dictionary<string, string> data, int badge = 1)
    {
        try
        {
            if (tokens.IsNullOrEmpty()) return;
            icon = icon.IsNullOrWhiteSpace() ? null : icon;
            var message = new MulticastMessage()
            {
                Notification = MessageHelper.GetNotification(title, content, icon),
                Tokens = tokens,
                Android = MessageHelper.GetAndroidConfig(badge),
                Apns = MessageHelper.GetApnsConfig(badge),
                Webpush = MessageHelper.GetWebPushConfig(badge),
                Data = data
            };

            var result = await FirebaseMessaging.DefaultInstance.SendMulticastAsync(message);
            _logger.LogDebug("multicast send, message: {message}, result: {result}", JsonConvert.SerializeObject(message), JsonConvert.SerializeObject(result));
            
            if (result == null)
            {
                _logger.LogError(
                    "multicast send firebase error, result is null, title:{title}, content:{content}", title, content);
                return;
            }

            _logger.LogDebug("multicast send success, title:{title}, content:{content}, successCount:{successCount}",
                title, content,
                result.SuccessCount);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "multicast send firebase exception, {token}, title: {title}, content:{content}", tokens,
                title, content);
        }
    }

    public async Task PushAsync(string token, string icon, string title, string content,
        Dictionary<string, string> data, int badge = 1)
    {
        try
        {
            if (token.IsNullOrEmpty()) return;
            icon = icon.IsNullOrWhiteSpace() ? null : icon;
            var message = new Message()
            {
                Notification = MessageHelper.GetNotification(title, content, icon),
                Token = token,
                Android = MessageHelper.GetAndroidConfig(badge),
                Apns = MessageHelper.GetApnsConfig(badge),
                Webpush = MessageHelper.GetWebPushConfig(badge),
                Data = data
            };

            var result = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.LogDebug("send firebase, message: {message}, result: {result}", JsonConvert.SerializeObject(message), JsonConvert.SerializeObject(result));

            if (result.IsNullOrEmpty())
            {
                _logger.LogError("send firebase error, result is null, title:{title}, content:{content}", title,
                    content);
                return;
            }

            _logger.LogDebug("send to firebase success, title:{title}, content:{content}", title, content);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "send firebase exception, {token}, title: {title}, content:{content}", token, title,
                content);
        }
    }
}