using Broker.Server.Infrastructure;
using Broker.Server.Services;
using Broker.Server.Services.Implementation;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews().AddNewtonsoftJson();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthHandler>();
builder.Services.AddSingleton<IAgentKeyService, AgentKeyService>();

builder.Services.AddScoped<IAgentFeedService, AgentFeedService>();
builder.Services.AddSingleton<IAgentFeedRepository, AgentFeedRepository>();

builder.Services.AddScoped<IBrokerFeedService, BrokerFeedService>();
builder.Services.AddSingleton<IBrokerFeedRepository, BrokerFeedRepository>();

builder.Services.AddSingleton<IRecentMessagesRepository, RecentMessagesRepository>();

//builder.Services.AddSingleton<IBindingRepository, BindingRepository>();
builder.Services.AddSingleton<IBindingRepository, BindingFileCacheRepository>();
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
