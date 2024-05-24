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
using Newtonsoft.Json;
using Volo.Abp;
using Volo.Abp.Auditing;

namespace MessagePush.DeviceInfo;

[RemoteService(false), DisableAuditing]
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
        await DeleteDeviceInfoAsync(input.DeviceId);
        var id = DeviceInfoHelper.GetId(input.UserId, input.DeviceId, input.NetworkType.ToString());
        var deviceInfo = ObjectMapper.Map<UserDeviceInfoDto, UserDeviceIndex>(input);
        deviceInfo.Id = id;
        deviceInfo.ModificationTime = DateTime.UtcNow;
        deviceInfo.AppStatus = AppStatus.Foreground.ToString();
        deviceInfo.AppId = _httpContextAccessor.HttpContext?.Request.Headers.GetOrDefault(CommonConstant.AppIdKeyName);

        await _deviceInfoRepository.AddOrUpdateAsync(deviceInfo);
        Logger.LogDebug("report device info, appId: {appId}, id: {id}", deviceInfo.AppId ?? string.Empty, id);
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
        await _deviceInfoRepository.AddOrUpdateAsync(deviceInfo);
        Logger.LogDebug("report app status, appId: {appId}, id: {id},  status: {status}",
            deviceInfo.AppId ?? string.Empty, id, deviceInfo.AppStatus);
    }


    private async Task DeleteDeviceInfoAsync(string deviceId)
    {
        var (totalCount, data) = await _userDeviceProvider.GetDeviceInfoListAsync(deviceId);
        if (totalCount == 0) return;

        await _deviceInfoRepository.BulkDeleteAsync(data);
    }

    private async void TryDeleteInvalidDeviceInfo(InvalidDeviceCriteria criteria)
    {
        if (criteria.LoginUserIds == null || !criteria.LoginUserIds.Any())
        {
            return;
        }

        var invalidDeviceInfos = await _userDeviceProvider.GetInvalidDeviceInfos(criteria);
        if (invalidDeviceInfos != null && invalidDeviceInfos.Any())
        {
            Logger.LogDebug("Invalid device attempts exist, trying to delete them.");
            foreach (var invalidDeviceInfo in invalidDeviceInfos)
            {
                await _userDeviceProvider.DeleteUserDeviceAsync(invalidDeviceInfo.Id);
                Logger.LogDebug("delete invalid device info, id: {id}, invalidDeviceInfo: {invalidDeviceInfo}",
                    invalidDeviceInfo.Id, JsonConvert.SerializeObject(invalidDeviceInfo));
            }
        }
    }

    public async Task ExitWalletAsync(ExitWalletDto input)
    {
        await DeleteDeviceInfoAsync(input.DeviceId);
        Logger.LogDebug("report exit wallet deviceId: {deviceId}", input.DeviceId);
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