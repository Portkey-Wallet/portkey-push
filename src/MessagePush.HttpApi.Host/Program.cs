using System.Threading.Tasks;
using MessagePush.Common;
using MessagePush.ScheduledTasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace MessagePush;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();
        Log.Logger = LogHelper.CreateLogger(LogEventLevel.Debug, configuration);

        try
        {
            Log.Information("Starting MessagePush.HttpApi.Host.");
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.AddAppSettingsSecretsJson()
                .UseApolloForConfigureHostBuilder()
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
        catch (System.Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly!");
            return 1;
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }
}
