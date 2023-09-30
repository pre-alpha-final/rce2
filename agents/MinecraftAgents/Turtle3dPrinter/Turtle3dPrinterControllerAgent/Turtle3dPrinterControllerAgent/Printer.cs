using System.Text.RegularExpressions;

namespace Turtle3dPrinterControllerAgent;

public class Printer
{
    private readonly Turtle _turtle;
    private readonly AppSettings _appSettings;

    public Printer(Turtle turtle, AppSettings appSettings)
    {
        _turtle = turtle;
        _appSettings = appSettings;
    }

    public async Task Run()
    {
        // TODO share with scanner somehow
        var colorMapping = new Dictionary<Rgba32, string>
        {
            { new Rgba32(255, 255, 255), "minecraft:white_wool" },
            { new Rgba32(0, 0, 0), "minecraft:black_wool" },
            { new Rgba32(255, 242, 0), "minecraft:yellow_wool" },
            { new Rgba32(255, 127, 39), "minecraft:orange_wool" },
        };

        _turtle.ResetOrigin();
        await _turtle.SendCustomCommand(@"creativeChest = peripheral.wrap(""left"")");

        var files = Directory.GetFiles(_appSettings.PrintFolder)
            .Select(e => (index: int.Parse(new Regex("\\d+(?!\\d+)").Match(e).Groups.Values.Last().Value), file: e))
            .OrderBy(e => e.index)
            .Select(e => e.file)
            .ToList();

        foreach (var file in files)
        {
            var image = await Image.LoadAsync<Rgba32>(file);
            await PrintLayer(image, colorMapping);
            await _turtle.Up();
        }
    }

    public async Task PrintLayer(Image<Rgba32> image, Dictionary<Rgba32, string> colorMappings)
    {
        for (int imageX = 0; ; imageX++)
        {
            for (int imageY = 0; ; imageY++)
            {
                var pixel = image[imageX, (imageX % 2 == 0) ? (image.Height - 1 - imageY) : imageY];
                if (colorMappings.ContainsKey(pixel))
                {
                    await _turtle.SendCustomCommand(@$"creativeChest.generate(""{colorMappings[pixel]}"", 1)");
                    await _turtle.SendCustomCommand(@"turtle.placeDown()");
                }
                if (imageY == image.Height - 1)
                {
                    break;
                }
                await _turtle.Forward();
            }
            if (imageX == image.Width - 1)
            {
                break;
            }

            if (imageX % 2 == 0)
            {
                await _turtle.TurnRight();
            }
            else
            {
                await _turtle.TurnLeft();
            }
            await _turtle.Forward();
            if (imageX % 2 == 0)
            {
                await _turtle.TurnRight();
            }
            else
            {
                await _turtle.TurnLeft();
            }
        }

        await _turtle.GoToOriginXY();
    }
}
