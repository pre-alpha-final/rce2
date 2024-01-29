using FileSyncAgent.Services;
using FileSyncAgent.Services.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rce2;

namespace FileSyncAgent;

public class Program
{
    static async Task Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddHostedService<App>();
        builder.Services.AddHttpClient();
        builder.Services.AddTransient<IConfigRepository, ConfigRepository>();
        builder.Services.AddSingleton<Rce2Service>();

        using var host = builder.Build();
        await host.RunAsync();
    }
}
