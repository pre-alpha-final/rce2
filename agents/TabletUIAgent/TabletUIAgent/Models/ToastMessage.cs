using PubSub;

namespace TabletUIAgent.Models;

/// <summary>
/// Severity of a toast, which selects its background colour.
/// </summary>
public enum ToastLevel
{
    /// <summary>A normal, informational toast (primary background).</summary>
    Normal,
    /// <summary>An error toast (red/danger background).</summary>
    Error
}

/// <summary>
/// A request to show a toast. Published on <see cref="Hub.Default"/> and rendered by
/// the <c>ToastHost</c> component. Prefer the <see cref="Toast"/> helper to raise these.
/// </summary>
public record ToastMessage(string Text, ToastLevel Level = ToastLevel.Normal);

/// <summary>
/// Convenience entry point for showing toasts from anywhere in the app. The toast is
/// published over the existing <see cref="Hub.Default"/> PubSub bus, so no service
/// injection is required — just call <c>Toast.Show("...")</c> or <c>Toast.Error("...")</c>.
/// </summary>
public static class Toast
{
    /// <summary>Shows a normal (primary) toast.</summary>
    public static void Show(string text) =>
        Hub.Default.Publish(new ToastMessage(text, ToastLevel.Normal));

    /// <summary>Shows an error (red) toast.</summary>
    public static void Error(string text) =>
        Hub.Default.Publish(new ToastMessage(text, ToastLevel.Error));
}
