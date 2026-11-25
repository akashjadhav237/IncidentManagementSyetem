using Microsoft.AspNetCore.Components;

namespace IncidentAgent.BlazorUi.Components;

/// <summary>Inline SVG icons (stroke = currentColor). Decorative: hidden from screen readers.</summary>
public static class Icons
{
    private static MarkupString Svg(string paths) => new(
        $"<svg class=\"icon\" viewBox=\"0 0 24 24\" width=\"20\" height=\"20\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"2\" stroke-linecap=\"round\" stroke-linejoin=\"round\" aria-hidden=\"true\" focusable=\"false\">{paths}</svg>");

    public static readonly MarkupString Logo = Svg("<path d=\"M12 2 3 6v6c0 5 3.8 9.3 9 10 5.2-.7 9-5 9-10V6l-9-4Z\"/><path d=\"M12 8v4\"/><path d=\"M12 16h.01\"/>");
    public static readonly MarkupString Dashboard = Svg("<rect x=\"3\" y=\"3\" width=\"7\" height=\"9\" rx=\"1\"/><rect x=\"14\" y=\"3\" width=\"7\" height=\"5\" rx=\"1\"/><rect x=\"14\" y=\"12\" width=\"7\" height=\"9\" rx=\"1\"/><rect x=\"3\" y=\"16\" width=\"7\" height=\"5\" rx=\"1\"/>");
    public static readonly MarkupString Incidents = Svg("<path d=\"M10.3 3.9 1.8 18a2 2 0 0 0 1.7 3h17a2 2 0 0 0 1.7-3L13.7 3.9a2 2 0 0 0-3.4 0Z\"/><path d=\"M12 9v4\"/><path d=\"M12 17h.01\"/>");
    public static readonly MarkupString User = Svg("<circle cx=\"12\" cy=\"8\" r=\"4\"/><path d=\"M4 21v-1a6 6 0 0 1 6-6h4a6 6 0 0 1 6 6v1\"/>");
    public static readonly MarkupString Tag = Svg("<path d=\"M20.6 13.4 13.4 20.6a2 2 0 0 1-2.8 0L3 13V3h10l7.6 7.6a2 2 0 0 1 0 2.8Z\"/><circle cx=\"7.5\" cy=\"7.5\" r=\"1.5\"/>");
    public static readonly MarkupString Server = Svg("<rect x=\"2\" y=\"3\" width=\"20\" height=\"8\" rx=\"2\"/><rect x=\"2\" y=\"13\" width=\"20\" height=\"8\" rx=\"2\"/><path d=\"M6 7h.01\"/><path d=\"M6 17h.01\"/>");
    public static readonly MarkupString Settings = Svg("<circle cx=\"12\" cy=\"12\" r=\"3\"/><path d=\"M19.4 15a1.7 1.7 0 0 0 .3 1.8l.1.1a2 2 0 1 1-2.8 2.8l-.1-.1a1.7 1.7 0 0 0-1.8-.3 1.7 1.7 0 0 0-1 1.5V21a2 2 0 1 1-4 0v-.1a1.7 1.7 0 0 0-1.1-1.5 1.7 1.7 0 0 0-1.8.3l-.1.1a2 2 0 1 1-2.8-2.8l.1-.1a1.7 1.7 0 0 0 .3-1.8 1.7 1.7 0 0 0-1.5-1H3a2 2 0 1 1 0-4h.1a1.7 1.7 0 0 0 1.5-1.1 1.7 1.7 0 0 0-.3-1.8l-.1-.1a2 2 0 1 1 2.8-2.8l.1.1a1.7 1.7 0 0 0 1.8.3H9a1.7 1.7 0 0 0 1-1.5V3a2 2 0 1 1 4 0v.1a1.7 1.7 0 0 0 1 1.5 1.7 1.7 0 0 0 1.8-.3l.1-.1a2 2 0 1 1 2.8 2.8l-.1.1a1.7 1.7 0 0 0-.3 1.8V9a1.7 1.7 0 0 0 1.5 1H21a2 2 0 1 1 0 4h-.1a1.7 1.7 0 0 0-1.5 1Z\"/>");
    public static readonly MarkupString Search = Svg("<circle cx=\"11\" cy=\"11\" r=\"7\"/><path d=\"m20 20-3.5-3.5\"/>");
    public static readonly MarkupString Refresh = Svg("<path d=\"M21 12a9 9 0 0 1-15.5 6.2L3 16\"/><path d=\"M3 12a9 9 0 0 1 15.5-6.2L21 8\"/><path d=\"M21 3v5h-5\"/><path d=\"M3 21v-5h5\"/>");
    public static readonly MarkupString Menu = Svg("<path d=\"M4 6h16\"/><path d=\"M4 12h16\"/><path d=\"M4 18h16\"/>");
    public static readonly MarkupString ChevronLeft = Svg("<path d=\"m15 18-6-6 6-6\"/>");
    public static readonly MarkupString ChevronRight = Svg("<path d=\"m9 18 6-6-6-6\"/>");
    public static readonly MarkupString ArrowLeft = Svg("<path d=\"M19 12H5\"/><path d=\"m12 19-7-7 7-7\"/>");
    public static readonly MarkupString Alert = Svg("<circle cx=\"12\" cy=\"12\" r=\"9\"/><path d=\"M12 8v4\"/><path d=\"M12 16h.01\"/>");
    public static readonly MarkupString Inbox = Svg("<path d=\"M22 12h-6l-2 3h-4l-2-3H2\"/><path d=\"M5.5 5.1 2 12v6a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2v-6l-3.5-6.9A2 2 0 0 0 16.8 4H7.2a2 2 0 0 0-1.7 1.1Z\"/>");
}
