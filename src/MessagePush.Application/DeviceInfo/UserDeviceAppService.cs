using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using MessagePush.Commons;
using MessagePush.DeviceInfo.Dtos;
using MessagePush.DeviceInfo.Provider;
using MessagePush.Entities.Es;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Nest;
using Newtonsoft.Json;
using Volo.Abp;

namespace MessagePush.DeviceInfo;

[RemoteService(false)]
public class UserDeviceAppService : MessagePushBaseService, IUserDeviceAppService
{
    private readonly INESTRepository<UserDeviceIndex, string> _deviceInfoRepository;
    private readonly IUserDeviceProvider _userDeviceProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserDeviceAppService(INESTRepository<UserDeviceIndex, string> deviceInfoRepository,
        IUserDeviceProvider userDeviceProvider, IHttpContextAccessor httpContextAccessor)
    {
        _deviceInfoRepository = deviceInfoRepository;
        _userDeviceProvider = userDeviceProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task ReportDeviceInfoAsync(UserDeviceInfoDto input)
    {
        Logger.LogInformation("report device info input, data:{data}", JsonConvert.SerializeObject(input));
        var id = DeviceInfoHelper.GetId(input.UserId, input.DeviceId, input.NetworkType.ToString());
        var deviceInfo = ObjectMapper.Map<UserDeviceInfoDto, UserDeviceIndex>(input);
        deviceInfo.Id = id;
        deviceInfo.ModificationTime = DateTime.UtcNow;
        deviceInfo.AppStatus = AppStatus.Foreground.ToString();
        deviceInfo.AppId = _httpContextAccessor.HttpContext?.Request.Headers.GetOrDefault(CommonConstant.AppIdKeyName);

        Logger.LogInformation("update index:{index}", JsonConvert.SerializeObject(deviceInfo));
        
        await Task.Delay(1000); // Delay for 1 second for testing
        
        await _deviceInfoRepository.AddOrUpdateAsync(deviceInfo);
        Logger.LogDebug("report device info, appId: {appId}, id: {id}", deviceInfo.AppId ?? string.Empty, id);
        
        TryDeleteInvalidDeviceInfo(InvalidDeviceCriteria.FromUserDeviceInfoDto(input));
    }
    
    public async Task ReportAppStatusAsync(ReportAppStatusDto input)
    {
        var id = DeviceInfoHelper.GetId(input.UserId, input.DeviceId, input.NetworkType.ToString());
        var deviceInfo = await _userDeviceProvider.GetDeviceInfoAsync(id);

        if (deviceInfo == null)
        {
            return;
        }

        deviceInfo.ModificationTime = DateTime.UtcNow;
        deviceInfo.AppStatus = input.Status.ToString();

        await _userDeviceProvider.UpdateUnreadInfoAsync(deviceInfo.AppId, input.UserId, input.UnreadCount);
        Logger.LogInformation("update status index:{index}", JsonConvert.SerializeObject(deviceInfo));
        await _deviceInfoRepository.AddOrUpdateAsync(deviceInfo);
        Logger.LogDebug("report app status, appId: {appId}, id: {id},  status: {status}",
            deviceInfo.AppId ?? string.Empty, id, deviceInfo.AppStatus);
        
        TryDeleteInvalidDeviceInfo(InvalidDeviceCriteria.FromReportAppStatusDto(input));
    }
    
    
    private async void TryDeleteInvalidDeviceInfo(InvalidDeviceCriteria criteria)
    {
        

        var invalidDeviceInfos = await GetInvalidDeviceInfos(criteria);
        if (invalidDeviceInfos != null && invalidDeviceInfos.Any())
        {
            Logger.LogDebug("Invalid device attempts exist, trying to delete them.");
            foreach (var invalidDeviceInfo in invalidDeviceInfos)
            {
                await _deviceInfoRepository.DeleteAsync(invalidDeviceInfo.Id);
                Logger.LogDebug("delete invalid device info, id: {id}, invalidDeviceInfo: {invalidDeviceInfo}", invalidDeviceInfo.Id, JsonConvert.SerializeObject(invalidDeviceInfo));
            }
        }
    }
    
    private async Task<List<UserDeviceIndex>> GetInvalidDeviceInfos(InvalidDeviceCriteria criteria)
    {
        var filter = new Func<QueryContainerDescriptor<UserDeviceIndex>, QueryContainer>(q =>
        {
            var baseQuery = q.Term(t => t.Field(f => f.DeviceId).Value(criteria.DeviceId)) &&
                            !q.Terms(t => t.Field(f => f.UserId).Terms(criteria.LoginUserIds.ToArray()));

            return baseQuery;
        });

        var result = await _deviceInfoRepository.GetListAsync(filter);
        return result.Item2;
    }

    public async Task ExitWalletAsync(ExitWalletDto input)
    {
        var id = DeviceInfoHelper.GetId(input.UserId, input.DeviceId, input.NetworkType.ToString());
        await _deviceInfoRepository.DeleteAsync(id);
        Logger.LogDebug("report exit wallet id: {id}", id);
    }

    public async Task SwitchNetworkAsync(SwitchNetworkDto input)
    {
        var id = DeviceInfoHelper.GetId(input.UserId, input.DeviceId, input.NetworkType.ToString());
        var deviceInfo = await _userDeviceProvider.GetDeviceInfoAsync(id);

        if (deviceInfo == null)
        {
            return;
        }

        deviceInfo.ModificationTime = DateTime.UtcNow;
        deviceInfo.AppStatus = AppStatus.Offline.ToString();

        await _deviceInfoRepository.AddOrUpdateAsync(deviceInfo);
        Logger.LogDebug("report switch network, appId: {appId}, id: {id},  status: {status}",
            deviceInfo.AppId ?? string.Empty, id, deviceInfo.AppStatus);
    }

    public async Task UpdateUnreadMessageAsync(UnreadMessageDto input)
    {
        var appId = GetFromHeader();
        await _userDeviceProvider.UpdateUnreadInfoAsync(appId, input.UserId, input.UnreadCount);

        Logger.LogDebug("update unread message, appId: {appId}, userId:{userId}, unreadCount: {unreadCount}",
            appId ?? string.Empty, input.UserId, input.UnreadCount);
    }

    private string GetFromHeader()
    {
        var appId = _httpContextAccessor.HttpContext?.Request.Headers.GetOrDefault(CommonConstant.AppIdKeyName);
        return appId;
    }
}