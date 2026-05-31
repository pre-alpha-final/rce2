using TabletUIAgent.Pages;

namespace TabletUIAgent.Models;

/// <summary>
/// Single source of truth for the main menu: drives both the cards rendered in
/// <c>MainMenu</c> and the routing performed by <c>Index</c>.
/// </summary>
public static class MenuCatalog
{
    public static readonly IReadOnlyList<MenuItem> Items = new[]
    {
        new MenuItem("youtube", "Youtube", "Youtube media control", typeof(Youtube)),
        new MenuItem("placeholder1", "Placeholder", "placeholder"),
        new MenuItem("placeholder2", "Placeholder", "placeholder"),
        new MenuItem("placeholder3", "Placeholder", "placeholder"),
        new MenuItem("sendit", "Send It!", "Send debug signals", typeof(SendIt)),
    };

    public static MenuItem? FindEnabled(string id) =>
        Items.FirstOrDefault(i => i.Id == id && i.Enabled);
}
