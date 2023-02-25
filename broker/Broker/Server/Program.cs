using Broker.Server.Infrastructure;
using Broker.Server.Services;
using Broker.Server.Services.Implementation;
using Broker.Shared.Events;
using Broker.Shared.Model;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews().AddNewtonsoftJson();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthHandler>();

builder.Services.AddScoped<IFeedService<Rce2Message>, FeedService<Rce2Message>>();
builder.Services.AddSingleton<IFeedRepository<Rce2Message>, FeedRepository<Rce2Message>>();
builder.Services.AddSingleton<IEchoFeedRepository<Rce2Message>, EchoFeedRepositoryDummy<Rce2Message>>();

builder.Services.AddScoped<IFeedService<BrokerEventBase>, FeedService<BrokerEventBase>>();
builder.Services.AddSingleton<IFeedRepository<BrokerEventBase>, FeedRepository<BrokerEventBase>>();
builder.Services.AddSingleton<IEchoFeedRepository<BrokerEventBase>, EchoFeedRepository<BrokerEventBase>>();

builder.Services.AddSingleton<IBindingRepository, BindingRepository>();
builder.Services.AddSingleton<IJanitorService, JanitorService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("All", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

_ = Task.Run(app.Services.GetService<IJanitorService>().Run);

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
app.UseCors("All");
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
