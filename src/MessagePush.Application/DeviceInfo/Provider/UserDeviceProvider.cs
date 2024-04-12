using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using MessagePush.Commons;
using MessagePush.Entities.Es;
using MessagePush.MessagePush.Provider;
using Nest;
using Volo.Abp.DependencyInjection;

namespace MessagePush.DeviceInfo.Provider;

public interface IUserDeviceProvider
{
    Task<UserDeviceIndex> GetDeviceInfoAsync(string id);
    Task DeleteUserDeviceAsync(string id);
    Task<UnreadMessageIndex> GetUnreadInfoAsync(string userId);
    Task UpdateUnreadInfoAsync(string appId, string userId, int unreadCount);
    Task<List<UserDeviceIndex>> GetInvalidDeviceInfos(InvalidDeviceCriteria criteria);
    Task<List<UserDeviceIndex>> GetExpiredDeviceInfos(ExpiredDeviceCriteria criteria);
}

public class UserDeviceProvider : IUserDeviceProvider, ISingletonDependency
{
    private readonly INESTRepository<UserDeviceIndex, string> _userDeviceRepository;
    private readonly INESTRepository<UnreadMessageIndex, string> _unreadMessageIndexRepository;
    private readonly IMessagePushProvider _messagePushProvider;

    public UserDeviceProvider(INESTRepository<UserDeviceIndex, string> userDeviceRepository,
        INESTRepository<UnreadMessageIndex, string> unreadMessageIndexRepository,
        IMessagePushProvider messagePushProvider)
    {
        _userDeviceRepository = userDeviceRepository;
        _unreadMessageIndexRepository = unreadMessageIndexRepository;
        _messagePushProvider = messagePushProvider;
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

    public async Task<UnreadMessageIndex> GetUnreadInfoAsync(string userId)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<UnreadMessageIndex>, QueryContainer>>()
        {
            descriptor => descriptor.Term(i => i.Field(f => f.Id).Value(userId))
        };

        QueryContainer Filter(QueryContainerDescriptor<UnreadMessageIndex> f) => f.Bool(b => b.Must(mustQuery));
        return await _unreadMessageIndexRepository.GetAsync(Filter);
    }

    public async Task UpdateUnreadInfoAsync(string appId, string userId, int unreadCount)
    {
        var unreadInfo = await GetUnreadInfoAsync(userId);
        // await CheckClearMessageAsync(unreadInfo, unreadCount, userId);

        if (unreadInfo == null)
        {
            unreadInfo = new UnreadMessageIndex()
            {
                Id = userId,
                UserId = userId,
                AppId = appId,
                UnreadMessageInfos = new List<UnreadMessageInfo>()
            };
        }

        if (unreadInfo.UnreadMessageInfos.IsNullOrEmpty())
        {
            unreadInfo.UnreadMessageInfos = new List<UnreadMessageInfo>
            {
                new UnreadMessageInfo
                {
                    MessageType = MessageType.RelationOne.ToString(),
                    UnreadCount = 0
                }
            };

            await _unreadMessageIndexRepository.AddOrUpdateAsync(unreadInfo);
            return;
        }

        var imMessage = unreadInfo.UnreadMessageInfos.FirstOrDefault(t =>
            t.MessageType.Equals(MessageType.RelationOne.ToString(), StringComparison.OrdinalIgnoreCase));
        if (imMessage == null)
        {
            unreadInfo.UnreadMessageInfos.Add(new UnreadMessageInfo()
            {
                MessageType = MessageType.RelationOne.ToString(),
                UnreadCount = unreadCount
            });
        }
        else
        {
            imMessage.UnreadCount = unreadCount;
        }

        await _unreadMessageIndexRepository.AddOrUpdateAsync(unreadInfo);
    }

    private async Task CheckClearMessageAsync(UnreadMessageIndex unreadInfo, int unreadCount, string userId)
    {
        if (unreadInfo == null || unreadInfo.UnreadMessageInfos.IsNullOrEmpty())
        {
            return;
        }

        if (unreadInfo.UnreadMessageInfos.Sum(t => t.UnreadCount) > 0 && unreadCount == 0)
        {
            //clear message of ios and extension
            var userDevices =
                await _messagePushProvider.GetUserDevicesAsync(new List<string>() { userId }, string.Empty);

            var androidDevices = userDevices.Where(t =>
                t.DeviceInfo.DeviceType.Equals(DeviceType.Android.ToString(), StringComparison.OrdinalIgnoreCase) ==
                false).ToList();

            await _messagePushProvider.BulkPushAsync(androidDevices, string.Empty, CommonConstant.DefaultTitle,
                CommonConstant.DefaultContent, new Dictionary<string, string>(), badge: 0);
        }
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