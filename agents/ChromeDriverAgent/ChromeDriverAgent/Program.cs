using ChromeDriverAgent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rce2;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHostedService<App>();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<Rce2Service>();

var host = builder.Build();

await host.RunAsync();
