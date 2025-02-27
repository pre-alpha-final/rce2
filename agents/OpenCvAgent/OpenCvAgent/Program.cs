using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenCvAgent.Infrastructure;
using Rce2;

namespace OpenCvAgent;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddHostedService<App>();
        builder.Services.AddHttpClient();
        builder.Services.AddSingleton<Rce2Service>();
        builder.Services.AddTransient<OpenCvService>();

        var host = builder.Build();

        await host.RunAsync();
    }
}
