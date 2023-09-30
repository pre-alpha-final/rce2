using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using Turtle3dPrinterControllerAgent.Rce2;

namespace Turtle3dPrinterControllerAgent;

public class Turtle
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AppSettings _appSettings;

    // Should all be vec3 math, don't care
    private int _x = 0;
    private int _y = 0;
    private int _z = 0;
    private int _facing = 0;

    public Turtle(IHttpClientFactory httpClientFactory, AppSettings appSettings)
    {
        _httpClientFactory = httpClientFactory;
        _appSettings = appSettings;
    }

    public void ResetOrigin()
    {
        _x = 0;
        _y = 0;
        _z = 0;
        _facing = 0;
    }

    public async Task Forward()
    {
        if (_facing == 0)
        {
            _y += 1;
        }
        else if (_facing == 90)
        {
            _x += 1;
        }
        else if (_facing == 180)
        {
            _y -= 1;
        }
        else if (_facing == 270)
        {
            _x -= 1;
        }

        await SendCustomCommand("turtle.forward()");
    }

    public async Task TurnRight()
    {
        _facing += 90;
        if (_facing == 360)
        {
            _facing = 0;
        }

        await SendCustomCommand("turtle.turnRight()");
    }

    public async Task TurnLeft()
    {
        _facing -= 90;
        if (_facing == -90)
        {
            _facing = 270;
        }

        await SendCustomCommand("turtle.turnLeft()");
    }

    public async Task Up()
    {
        _z += 1;
        await SendCustomCommand("turtle.up()");
    }

    public async Task Down()
    {
        _z -= 1;
        await SendCustomCommand("turtle.down()");
    }

    public async Task GoToOriginXY()
    {
        if (_x > 0)
        {
            while (_facing != 270)
            {
                await TurnLeft();
            }
        }
        if (_x < 0)
        {
            while (_facing != 90)
            {
                await TurnLeft();
            }
        }
        while (_x != 0)
        {
            await Forward();
        }

        if (_y > 0)
        {
            while (_facing != 180)
            {
                await TurnLeft();
            }
        }
        if (_y < 0)
        {
            while (_facing != 0)
            {
                await TurnLeft();
            }
        }
        while (_y != 0)
        {
            await Forward();
        }

        while (_facing != 0)
        {
            await TurnLeft();
        }
    }

    public async Task SendCustomCommand(string command)
    {
        try
        {
            await _httpClientFactory.CreateClient().PostAsync(_appSettings.Address, new StringContent(JsonConvert.SerializeObject(new Rce2Message
            {
                Type = Rce2Types.Number,
                Contact = Rce2Contacts.Outs.SendCommand,
                Payload = JObject.FromObject(new { data = command })
            }), Encoding.UTF8, "application/json"));
        }
        catch
        {
            // ignore
        }
    }
}
