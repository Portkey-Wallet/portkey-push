using System;
using System.Linq;
using System.Threading.Tasks;
using MessagePush.Commons;
using MessagePush.DeviceInfo.Provider;
using MessagePush.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Volo.Abp.Application.Services;

namespace MessagePush.ScheduledTasks.Jobs;

using Quartz;

public class DeleteExpiredDeviceInfoJob : ApplicationService, IJob
{
    private readonly IUserDeviceProvider _userDeviceProvider;
    private readonly ScheduledTasksOptions _scheduledTasks;

    public DeleteExpiredDeviceInfoJob(IUserDeviceProvider userDeviceProvider, IOptionsSnapshot<ScheduledTasksOptions> scheduledTasks)
    {
        _userDeviceProvider = userDeviceProvider;
        _scheduledTasks = scheduledTasks.Value;
    }

    // This job is designed to run on a single instance (single server, single application).
    // If the application is deployed in a cluster in the future, this job may need to be modified to work correctly in a clustered environment.
    public async Task Execute(IJobExecutionContext context)
    {
        var criteria = new ExpiredDeviceCriteria()
        {
            FromDays = _scheduledTasks.ExpiredDeviceInfoFromDays,
            Limit = _scheduledTasks.ExpiredDeviceInfoLimit
        };

        var expiredDeviceInfos = await _userDeviceProvider.GetExpiredDeviceInfos(criteria);

        while (expiredDeviceInfos != null && expiredDeviceInfos.Any())
        {
            foreach (var expiredDeviceInfo in expiredDeviceInfos)
            {
                Logger.LogInformation($"Starting to delete expired device info with ID: {expiredDeviceInfo.Id}, Expired device info: {JsonConvert.SerializeObject(expiredDeviceInfo)}");
                await _userDeviceProvider.DeleteUserDeviceAsync(expiredDeviceInfo.Id);
            }

            await Task.Delay(TimeSpan.FromMilliseconds(_scheduledTasks.DelayFromMilliseconds));

            // Check if there are more devices to delete
            if (expiredDeviceInfos.Count == criteria.Limit)
            {
                // If the previous query returned the maximum number of devices, there might be more devices to delete
                expiredDeviceInfos = await _userDeviceProvider.GetExpiredDeviceInfos(criteria);
            }
            else
            {
                // If the previous query returned less than the maximum number of devices, there are no more devices to delete
                expiredDeviceInfos = null;
            }
        }
    }
}