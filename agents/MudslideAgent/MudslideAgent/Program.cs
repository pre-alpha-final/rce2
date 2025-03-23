using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rce2;

namespace MudslideAgent;

internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddHostedService<App>();
        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<Rce2Service>();
        builder.Services.AddSingleton<MudslideService>();

        var host = builder.Build();

        await host.RunAsync();
    }
}
