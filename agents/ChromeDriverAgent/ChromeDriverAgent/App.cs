using Microsoft.Extensions.Hosting;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using PubSub;
using Rce2;

namespace ChromeDriverAgent;

public sealed class App : IHostedService
{
    private const string BrokerAddress = "https://localhost:7113";
    private const string AgentKey = "";
    private const string OpenUrlContact = "open-url";

    private readonly Rce2Service _rce2Service;
    private readonly object _driverLock = new();
    private readonly string _userDataDirectory = CreateUserDataDirectory();
    private ChromeDriver? _driver;

    public App(Rce2Service rce2Service)
    {
        _rce2Service = rce2Service;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _rce2Service
            .SetBrokerAddress(BrokerAddress)
            .SetAgentId(Guid.NewGuid())
            .SetAgentKey(AgentKey)
            .SetAgentName("Chrome Driver")
            .SetInputDefinitions(new()
            {
                { OpenUrlContact, Rce2Types.String }
            })
            .SetOutputDefinitions(new())
            .Init();

        Hub.Default.Subscribe<Rce2Message>(this, HandleMessage);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Hub.Default.Unsubscribe(this);

        lock (_driverLock)
        {
            try
            {
                _driver?.Quit();
                _driver?.Dispose();
            }
            finally
            {
                _driver = null;
            }
        }

        return Task.CompletedTask;
    }

    private void HandleMessage(Rce2Message message)
    {
        if (message.Contact != OpenUrlContact)
        {
            return;
        }

        var url = NormalizeUrl(message.Payload["data"]?.ToObject<string>());
        if (url is null)
        {
            Console.Error.WriteLine($"Ignoring invalid URL payload: {message.Payload["data"]}");
            return;
        }

        lock (_driverLock)
        {
            try
            {
                GetDriver().Navigate().GoToUrl(url);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Failed to open URL in Chrome: {ex}");
            }
        }
    }

    private ChromeDriver GetDriver()
    {
        if (_driver is not null)
        {
            return _driver;
        }

        var options = CreateChromeOptions();
        var driverPath = new DriverFinder(options).GetDriverPath();
        var driverDirectory = Path.GetDirectoryName(driverPath);
        var driverFileName = Path.GetFileName(driverPath);
        if (string.IsNullOrWhiteSpace(driverDirectory) || string.IsNullOrWhiteSpace(driverFileName))
        {
            throw new InvalidOperationException($"Selenium Manager returned an invalid ChromeDriver path: {driverPath}");
        }

        var service = ChromeDriverService.CreateDefaultService(driverDirectory, driverFileName);
        service.HideCommandPromptWindow = true;

        _driver = new ChromeDriver(service, options);
        return _driver;
    }

    private ChromeOptions CreateChromeOptions()
    {
        var options = new ChromeOptions();
        options.AddArgument($"--user-data-dir={_userDataDirectory}");
        options.AddArgument("--no-first-run");
        options.AddArgument("--no-default-browser-check");
        return options;
    }

    private static string? NormalizeUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Contains("://") == false)
        {
            trimmed = $"https://{trimmed}";
        }

        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
            string.IsNullOrWhiteSpace(uri.Host) == false)
        {
            return uri.AbsoluteUri;
        }

        return null;
    }

    private static string CreateUserDataDirectory()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var directory = Path.Combine(
            localAppData,
            "Rce2",
            "ChromeDriverAgent",
            "User Data");

        Directory.CreateDirectory(directory);
        return directory;
    }
}
