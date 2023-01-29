using Broker.Server.Infrastructure;
using Broker.Server.Services;
using Broker.Server.Services.Implementation;
using Broker.Shared.Events;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthHandler>();
builder.Services.AddScoped<IFeedService<BrokerEventBase>, FeedService<BrokerEventBase>>();
builder.Services.AddSingleton<IFeedRepository<BrokerEventBase>, FeedRepository<BrokerEventBase>>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
