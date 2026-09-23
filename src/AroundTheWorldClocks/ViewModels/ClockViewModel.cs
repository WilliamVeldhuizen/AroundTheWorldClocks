using System.Windows;
using System.Windows.Controls;
using AroundTheWorldClocks.Models;

namespace AroundTheWorldClocks.ViewModels;

public enum SkyPhase
{
    Night,
    Dawn,
    Day,
    Dusk,
}

public class ClockViewModel : ObservableObject
{
    private readonly TimeZoneInfo? _timeZone;
    private readonly bool _use24Hour;

    private string _timeText = "";
    private string _hourText = "";
    private string _minuteText = "";
    private string _dayOffsetText = "";
    private bool _isNight;

    public ClockViewModel(CityClock clock, WidgetSettings settings)
    {
        DisplayName = clock.DisplayName;
        Theme = settings.Theme;
        _use24Hour = settings.Use24HourFormat;
        ShowSecondHand = settings.ShowSecondHand;
        ClockSize = settings.ClockSize;
        IsVerticalWidget = settings.Orientation == WidgetOrientation.Vertical;

        try
        {
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById(clock.TimeZoneId);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            _timeZone = null;
        }

        Update(DateTimeOffset.UtcNow);
    }

    /// <summary>Raised after every <see cref="Update"/>; dial controls redraw on it.</summary>
    public event EventHandler? Updated;

    public string DisplayName { get; }
    public WidgetTheme Theme { get; }
    public bool ShowSecondHand { get; }
    public bool Use24Hour => _use24Hour;

    // ---- Layout (fixed per settings; the view model is rebuilt when settings change) ----

    public double ClockSize { get; }
    public double SmallClockSize => Math.Max(24, Math.Round(ClockSize * 0.47));
    public double FlipFontSize => Math.Max(14, Math.Round(ClockSize * 0.375));
    public bool IsVerticalWidget { get; }

    /// <summary>Clock and labels stack vertically in a horizontal widget, and side by side in a vertical one.</summary>
    public Orientation ItemOrientation => IsVerticalWidget ? Orientation.Horizontal : Orientation.Vertical;
    public HorizontalAlignment LabelAlignment => IsVerticalWidget ? HorizontalAlignment.Left : HorizontalAlignment.Center;
    public TextAlignment LabelTextAlignment => IsVerticalWidget ? TextAlignment.Left : TextAlignment.Center;
    public Thickness LabelMargin => IsVerticalWidget ? new Thickness(10, 0, 4, 0) : new Thickness(0, 5, 0, 0);

    // ---- Time ----

    public int Hour { get; private set; }
    public int Minute { get; private set; }
    public int Second { get; private set; }
    public SkyPhase Phase { get; private set; }

    public double HourAngle => (Hour % 12) * 30 + Minute * 0.5;
    public double MinuteAngle => Minute * 6 + Second * 0.1;
    public double SecondAngle => Second * 6;

    public string TimeText { get => _timeText; private set => SetProperty(ref _timeText, value); }
    public string HourText { get => _hourText; private set => SetProperty(ref _hourText, value); }
    public string MinuteText { get => _minuteText; private set => SetProperty(ref _minuteText, value); }

    /// <summary>"+1d" / "-1d" when the city's date differs from the local date, otherwise empty.</summary>
    public string DayOffsetText { get => _dayOffsetText; private set => SetProperty(ref _dayOffsetText, value); }

    /// <summary>True between 18:00 and 06:00 in the city.</summary>
    public bool IsNight { get => _isNight; private set => SetProperty(ref _isNight, value); }

    public void Update(DateTimeOffset utcNow)
    {
        if (_timeZone is null)
        {
            TimeText = "unknown zone";
            HourText = MinuteText = "--";
            Updated?.Invoke(this, EventArgs.Empty);
            return;
        }

        var cityTime = TimeZoneInfo.ConvertTime(utcNow, _timeZone);
        var localTime = utcNow.ToLocalTime();

        Hour = cityTime.Hour;
        Minute = cityTime.Minute;
        Second = cityTime.Second;
        Phase = Hour switch
        {
            < 5 or >= 21 => SkyPhase.Night,
            < 8 => SkyPhase.Dawn,
            < 17 => SkyPhase.Day,
            _ => SkyPhase.Dusk,
        };

        TimeText = cityTime.ToString(_use24Hour ? "HH:mm" : "h:mm tt");
        HourText = cityTime.ToString(_use24Hour ? "HH" : "hh");
        MinuteText = cityTime.ToString("mm");
        IsNight = Hour < 6 || Hour >= 18;

        int dayDiff = (cityTime.Date - localTime.Date).Days;
        DayOffsetText = dayDiff switch
        {
            0 => "",
            > 0 => $" +{dayDiff}d",
            _ => $" {dayDiff}d",
        };

        Updated?.Invoke(this, EventArgs.Empty);
    }
}
