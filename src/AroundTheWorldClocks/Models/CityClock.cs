namespace AroundTheWorldClocks.Models;

/// <summary>
/// One clock on the widget: a display name and a time zone id.
/// The id may be a Windows id ("W. Europe Standard Time") or an IANA id ("Europe/Amsterdam").
/// </summary>
public class CityClock
{
    public string DisplayName { get; set; } = "";
    public string TimeZoneId { get; set; } = "";

    public CityClock() { }

    public CityClock(string displayName, string timeZoneId)
    {
        DisplayName = displayName;
        TimeZoneId = timeZoneId;
    }

    public CityClock Clone() => new(DisplayName, TimeZoneId);

    public override string ToString() => $"{DisplayName}  ({TimeZoneId})";
}
