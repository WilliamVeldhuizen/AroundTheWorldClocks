namespace AroundTheWorldClocks.Models;

/// <summary>
/// Visual theme of the widget. Each value has a matching resource dictionary in Themes/{name}.xaml
/// and, for analog themes, a dial renderer in Rendering/ClockRenderer.cs.
/// </summary>
public enum WidgetTheme
{
    MicaGlass,
    SwissRailway,
    RomanClassic,
    SquircleSky,
    MidCentury,
    CompactStrip,
    InsetDigital,
    RetroFlip,
}

public record ThemeOption(WidgetTheme Theme, string Name);

public static class WidgetThemes
{
    public static IReadOnlyList<ThemeOption> All { get; } =
    [
        new(WidgetTheme.MicaGlass, "Mica Glass"),
        new(WidgetTheme.SwissRailway, "Swiss Railway"),
        new(WidgetTheme.RomanClassic, "Roman Classic"),
        new(WidgetTheme.SquircleSky, "Squircle Sky"),
        new(WidgetTheme.MidCentury, "Mid-century"),
        new(WidgetTheme.CompactStrip, "Compact Strip"),
        new(WidgetTheme.InsetDigital, "Inset Digital"),
        new(WidgetTheme.RetroFlip, "Retro Flip"),
    ];
}
