using PushNotificationAgent.Server.Services;
using PushNotificationAgent.Server.Services.Implementation;

namespace PushNotificationAgent.Server;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        builder.Services.AddSingleton<IRce2Service, Rce2Service>();
        builder.Services.AddSingleton<INotificationsService, NotificationsService>();

        var app = builder.Build();

        _ = Task.Run(() => app.Services.GetService<IRce2Service>()?.Run());

        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        app.UseRouting();

        app.MapRazorPages();
        app.MapControllers();
        app.MapFallbackToFile("index.html");

        app.Run();
    }
}
