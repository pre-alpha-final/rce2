using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rce2;

namespace ChromeLikeWaterfallAgent;

public class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Logging.ClearProviders();
        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<Rce2Service>();
        builder.Services.AddHostedService<App>();

        var host = builder.Build();

        host.Services.GetService<Rce2Service>()!.Run();

        await host.RunAsync();
    }
}
