namespace AroundTheWorldClocks.Models;

public enum WidgetOrientation
{
    Horizontal,
    Vertical,
}

public class WidgetSettings
{
    public List<CityClock> Clocks { get; set; } = [];

    public WidgetTheme Theme { get; set; } = WidgetTheme.MicaGlass;

    /// <summary>Diameter of each analog clock in device-independent pixels.</summary>
    public double ClockSize { get; set; } = 64;

    public bool Use24HourFormat { get; set; } = true;
    public bool ShowSecondHand { get; set; } = true;
    public bool AlwaysOnTop { get; set; }
    public WidgetOrientation Orientation { get; set; } = WidgetOrientation.Horizontal;

    /// <summary>Last window position; null means "use default placement".</summary>
    public double? Left { get; set; }
    public double? Top { get; set; }

    public static WidgetSettings CreateDefault() => new()
    {
        Clocks =
        [
            new CityClock("Amsterdam", "Europe/Amsterdam"),
            new CityClock("London", "Europe/London"),
            new CityClock("New York", "America/New_York"),
            new CityClock("Tokyo", "Asia/Tokyo"),
        ],
    };

    public WidgetSettings Clone() => new()
    {
        Clocks = Clocks.Select(c => c.Clone()).ToList(),
        Theme = Theme,
        ClockSize = ClockSize,
        Use24HourFormat = Use24HourFormat,
        ShowSecondHand = ShowSecondHand,
        AlwaysOnTop = AlwaysOnTop,
        Orientation = Orientation,
        Left = Left,
        Top = Top,
    };
}
