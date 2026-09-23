using AroundTheWorldClocks.Models;

namespace AroundTheWorldClocks.ViewModels;

public class ClockViewModel : ObservableObject
{
    private readonly TimeZoneInfo? _timeZone;
    private readonly bool _use24Hour;

    private double _hourAngle;
    private double _minuteAngle;
    private double _secondAngle;
    private string _timeText = "";
    private string _dayOffsetText = "";
    private bool _isNight;

    public ClockViewModel(CityClock clock, bool use24Hour, bool showSecondHand)
    {
        DisplayName = clock.DisplayName;
        _use24Hour = use24Hour;
        ShowSecondHand = showSecondHand;

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

    public string DisplayName { get; }
    public bool ShowSecondHand { get; }

    public double HourAngle { get => _hourAngle; private set => SetProperty(ref _hourAngle, value); }
    public double MinuteAngle { get => _minuteAngle; private set => SetProperty(ref _minuteAngle, value); }
    public double SecondAngle { get => _secondAngle; private set => SetProperty(ref _secondAngle, value); }
    public string TimeText { get => _timeText; private set => SetProperty(ref _timeText, value); }

    /// <summary>"+1d" / "-1d" when the city's date differs from the local date, otherwise empty.</summary>
    public string DayOffsetText { get => _dayOffsetText; private set => SetProperty(ref _dayOffsetText, value); }

    /// <summary>True between 18:00 and 06:00 in the city; switches the dial to a dark face.</summary>
    public bool IsNight { get => _isNight; private set => SetProperty(ref _isNight, value); }

    public void Update(DateTimeOffset utcNow)
    {
        if (_timeZone is null)
        {
            TimeText = "unknown zone";
            return;
        }

        var cityTime = TimeZoneInfo.ConvertTime(utcNow, _timeZone);
        var localTime = utcNow.ToLocalTime();

        SecondAngle = cityTime.Second * 6;
        MinuteAngle = cityTime.Minute * 6 + cityTime.Second * 0.1;
        HourAngle = (cityTime.Hour % 12) * 30 + cityTime.Minute * 0.5;

        TimeText = cityTime.ToString(_use24Hour ? "HH:mm" : "h:mm tt");
        IsNight = cityTime.Hour < 6 || cityTime.Hour >= 18;

        int dayDiff = (cityTime.Date - localTime.Date).Days;
        DayOffsetText = dayDiff switch
        {
            0 => "",
            > 0 => $"+{dayDiff}d",
            _ => $"{dayDiff}d",
        };
    }
}
