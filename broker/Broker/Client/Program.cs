using Broker.Client;
using Broker.Client.Infrastructure;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddHttpClient("Default", httpClient =>
        {
            httpClient.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
        });
        builder.Services.AddSingleton<AuthorizedHttpClientFactory>();

        await builder.Build().RunAsync();
    }
}
