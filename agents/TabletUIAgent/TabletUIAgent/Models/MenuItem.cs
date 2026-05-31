namespace TabletUIAgent.Models;

/// <summary>
/// A single main-menu entry. <see cref="Component"/> is the page rendered when the
/// card is tapped; a null component marks a not-yet-implemented placeholder, which
/// renders disabled and is not navigable.
/// </summary>
public record MenuItem(string Id, string Title, string Text, Type? Component = null)
{
    public bool Enabled => Component is not null;
}
