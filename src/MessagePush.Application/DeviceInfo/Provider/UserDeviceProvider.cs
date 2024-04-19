using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using MessagePush.Commons;
using MessagePush.Entities.Es;
using MessagePush.Entities.Redis;
using MessagePush.MessagePush.Provider;
using MessagePush.Redis;
using Nest;
using Volo.Abp.DependencyInjection;

namespace MessagePush.DeviceInfo.Provider;

public interface IUserDeviceProvider
{
    Task<UserDeviceIndex> GetDeviceInfoAsync(string id);
    Task DeleteUserDeviceAsync(string id);
    Task UpdateUnreadInfoAsync(string appId, string userId, int unreadCount);
    Task<List<UserDeviceIndex>> GetInvalidDeviceInfos(InvalidDeviceCriteria criteria);
    Task<List<UserDeviceIndex>> GetExpiredDeviceInfos(ExpiredDeviceCriteria criteria);
}

public class UserDeviceProvider : IUserDeviceProvider, ISingletonDependency
{
    private readonly INESTRepository<UserDeviceIndex, string> _userDeviceRepository;
    private readonly IMessagePushProvider _messagePushProvider;
    private readonly RedisClient _redisClient;

    public UserDeviceProvider(INESTRepository<UserDeviceIndex, string> userDeviceRepository,
        IMessagePushProvider messagePushProvider, RedisClient redisClient)
    {
        _userDeviceRepository = userDeviceRepository;
        _messagePushProvider = messagePushProvider;
        _redisClient = redisClient;
    }

    public async Task<UserDeviceIndex> GetDeviceInfoAsync(string id)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<UserDeviceIndex>, QueryContainer>>() { };
        mustQuery.Add(q => q.Term(i => i.Field(f => f.Id).Value(id)));

        QueryContainer Filter(QueryContainerDescriptor<UserDeviceIndex> f) => f.Bool(b => b.Must(mustQuery));
        return await _userDeviceRepository.GetAsync(Filter);
    }

    public async Task DeleteUserDeviceAsync(string id)
    {
        await _userDeviceRepository.DeleteAsync(id);
    }

    public async Task<UnreadMessage> GetUnreadInfoAsync(string userId)
    {
        return await Task.Run(() =>
        {
            var unreadMessage = new UnreadMessage()
            {
                UserId = userId,
                AppId = "PortKey",
                MessageType = MessageType.RelationOne.ToString()
            };

            var unreadCount = _redisClient.Get(unreadMessage.GetKey());
            unreadMessage.UnreadCount = unreadCount;
            return unreadMessage;
        });
    }

    public async Task UpdateUnreadInfoAsync(string appId, string userId, int unreadCount)
    {
        var unreadMessage = await GetUnreadInfoAsync(userId);
        // await CheckClearMessageAsync(unreadInfo, unreadCount, userId);

        if (unreadMessage == null)
        {
            unreadMessage = new UnreadMessage()
            {
                UserId = userId,
                AppId = appId,
                MessageType = MessageType.RelationOne.ToString(),
                UnreadCount = unreadCount,
            };
            _redisClient.AddIfNotExists(unreadMessage.GetKey(), unreadMessage.UnreadCount);
        }
        else
        {
            unreadMessage.UnreadCount = unreadCount;
        }

        _redisClient.Set(unreadMessage.GetKey(), unreadMessage.UnreadCount);
    }

    public async Task<List<UserDeviceIndex>> GetInvalidDeviceInfos(InvalidDeviceCriteria criteria)
    {
        var filter = new Func<QueryContainerDescriptor<UserDeviceIndex>, QueryContainer>(q =>
        {
            var baseQuery = q.Term(t => t.Field(f => f.DeviceId).Value(criteria.DeviceId)) &&
                            !q.Terms(t => t.Field(f => f.UserId).Terms(criteria.LoginUserIds.ToArray()));

            return baseQuery;
        });

        var result = await _userDeviceRepository.GetListAsync(filter);
        return result.Item2;
    }

    public async Task<List<UserDeviceIndex>> GetExpiredDeviceInfos(ExpiredDeviceCriteria criteria)
    {
        var filter = new Func<QueryContainerDescriptor<UserDeviceIndex>, QueryContainer>(q =>
            q.DateRange(r => r
                .Field(f => f.ModificationTime)
                .LessThan(DateMath.Now.Subtract(TimeSpan.FromDays(criteria.FromDays)))
            )
        );

        var sort = new Func<SortDescriptor<UserDeviceIndex>, IPromise<IList<ISort>>>(s =>
            s.Ascending(f => f.ModificationTime)
        );

        var result = await _userDeviceRepository.GetSortListAsync(filter, sortFunc: sort, limit: criteria.Limit);
        return result.Item2;
    }
}