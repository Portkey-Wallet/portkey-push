using System;
using System.Threading.Tasks;
using MessagePush.Common;
using MessagePush.ScheduledTasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace MessagePush;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        Log.Logger = LogHelper.CreateLogger(LogEventLevel.Debug);

        try
        {
            Log.Information("Starting MessagePush.HttpApi.Host.");
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.AddAppSettingsSecretsJson()
                .UseAutofac()
                .UseSerilog();
            builder.Services.AddSignalR();
            builder.Services.AddHostedService<QuartzStartup>();
            await builder.AddApplicationAsync<MessagePushHttpApiHostModule>();
            var app = builder.Build();
            await app.InitializeApplicationAsync();
            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
