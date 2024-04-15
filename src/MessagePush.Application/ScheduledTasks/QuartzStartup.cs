using System;
using MessagePush.Options;
using MessagePush.ScheduledTasks.Jobs;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Volo.Abp.Application.Services;
using Volo.Abp.Auditing;

namespace MessagePush.ScheduledTasks;

using Quartz;
using Quartz.Impl;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

[DisableAuditing]
public class QuartzStartup : ApplicationService, IHostedService
{
    
    private readonly ScheduledTasksOptions _scheduledTasks;
    private readonly IServiceProvider _serviceProvider;

    public QuartzStartup(IOptionsSnapshot<ScheduledTasksOptions> scheduledTasks, IServiceProvider serviceProvider)
    {
        _scheduledTasks = scheduledTasks.Value;
        _serviceProvider = serviceProvider;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation(_serviceProvider.GetService(typeof(DeleteExpiredDeviceInfoJob)).ToString());
        Logger.LogInformation("Quartz service is starting");
        IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;
        scheduler.JobFactory = new QuartzJobFactory(_serviceProvider); // Set the custom JobFactory
        scheduler.Start();

        IJobDetail job = JobBuilder.Create<DeleteExpiredDeviceInfoJob>().Build();
        
        ITrigger trigger = TriggerBuilder.Create()
            .WithDailyTimeIntervalSchedule
            (s =>
                s.WithIntervalInHours(24)
                    .OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(
                        _scheduledTasks.ExecutionHour, 0))
            )
            .Build();

        scheduler.ScheduleJob(job, trigger);

        Logger.LogInformation("Quartz service has started");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Quartz service is stopping");
        
        Logger.LogInformation("Quartz service has stopped");
        return Task.CompletedTask;
    }
}