using System.Windows;
using System.Windows.Media;
using AroundTheWorldClocks.Models;
using AroundTheWorldClocks.ViewModels;
using static AroundTheWorldClocks.Rendering.Dial;

namespace AroundTheWorldClocks.Rendering;

/// <summary>Draws the analog dial for each theme. Retro Flip has no dial; its tiles are plain XAML.</summary>
internal static class ClockRenderer
{
    private static readonly Typeface Georgia = new("Georgia");
    private static readonly Typeface SegoeSemibold = new(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal);
    private static readonly Typeface Bahnschrift = new(new FontFamily("Bahnschrift, Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

    private static readonly string[] RomanNumerals = ["XII", "I", "II", "III", "IIII", "V", "VI", "VII", "VIII", "IX", "X", "XI"];
    private static readonly string[] ArabicNumerals = ["12", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11"];

    public static void Render(Dial d, ClockViewModel c)
    {
        switch (c.Theme)
        {
            case WidgetTheme.MicaGlass: MicaGlass(d, c); break;
            case WidgetTheme.SwissRailway: SwissRailway(d, c); break;
            case WidgetTheme.RomanClassic: RomanClassic(d, c); break;
            case WidgetTheme.SquircleSky: SquircleSky(d, c); break;
            case WidgetTheme.MidCentury: MidCentury(d, c); break;
            case WidgetTheme.CompactStrip: CompactStrip(d, c); break;
            case WidgetTheme.InsetDigital: InsetDigital(d, c); break;
        }
    }

    // ---- Mica Glass ----
    private static readonly Brush MicaFaceDay = Brush("#F5FFFFFF"), MicaFaceNight = Brush("#EB181C26");
    private static readonly Brush MicaInkDay = Brush("#1B1F27"), MicaInkNight = Brush("#E8ECF2"), MicaAccent = Brush("#4CC2FF");

    private static void MicaGlass(Dial d, ClockViewModel c)
    {
        var ink = c.IsNight ? MicaInkNight : MicaInkDay;
        d.Circle(46, c.IsNight ? MicaFaceNight : MicaFaceDay);
        d.Ticks(12, 41, 36, 2.2, ink);
        d.Hand(c.HourAngle, 22, 5, ink);
        d.Hand(c.MinuteAngle, 33, 3.2, ink);
        if (c.ShowSecondHand)
            d.Hand(c.SecondAngle, 36, 1.4, MicaAccent, tail: 9);
        d.Circle(3, MicaAccent);
    }

    // ---- Swiss Railway ----
    private static readonly Brush SwissFace = Brush("#FFFFFF"), SwissRim = Brush("#B9BCC2"), SwissInk = Brush("#111111"), SwissRed = Brush("#D6201B");

    private static void SwissRailway(Dial d, ClockViewModel c)
    {
        d.Circle(47, SwissFace, SwissRim, 2);
        d.Ticks(60, 44, 41, 1.3, SwissInk);
        d.Ticks(12, 44, 33, 4.2, SwissInk);
        d.Hand(c.HourAngle, 26, 6.5, SwissInk, tail: 9, cap: PenLineCap.Flat);
        d.Hand(c.MinuteAngle, 40, 4.6, SwissInk, tail: 9, cap: PenLineCap.Flat);
        if (c.ShowSecondHand)
        {
            d.Rotated(c.SecondAngle, dc =>
            {
                dc.DrawLine(new Pen(SwissRed, 1.6), new Point(50, 64), new Point(50, 22));
                dc.DrawEllipse(SwissRed, null, new Point(50, 20), 5, 5);
            });
        }
    }

    // ---- Roman Classic ----
    private static readonly Brush RomanFaceDay = Brush("#FBF8EF"), RomanFaceNight = Brush("#ECE5D2");
    private static readonly Brush RomanInk = Brush("#1D1D1D"), RomanBlue = Brush("#1C3F94");

    private static void RomanClassic(Dial d, ClockViewModel c)
    {
        d.Circle(46, c.IsNight ? RomanFaceNight : RomanFaceDay, RomanInk, 1.2);
        d.Circle(41.5, null, RomanInk, 0.4);
        d.Ticks(60, 46, 43.5, 0.45, RomanInk);
        d.Numerals(RomanNumerals, 34.5, 7.6, RomanInk, Georgia);

        // Blued-steel "moon" hands: a line with a hollow ring near the tip.
        var ring = new Pen(RomanBlue, 1.5);
        d.Rotated(c.HourAngle, dc =>
        {
            dc.DrawLine(new Pen(RomanBlue, 2.2) { EndLineCap = PenLineCap.Round }, Center, new Point(50, 31));
            dc.DrawEllipse(null, ring, new Point(50, 35), 3.4, 3.4);
        });
        d.Rotated(c.MinuteAngle, dc =>
        {
            dc.DrawLine(new Pen(RomanBlue, 1.6) { EndLineCap = PenLineCap.Round }, Center, new Point(50, 15));
            dc.DrawEllipse(null, new Pen(RomanBlue, 1.3), new Point(50, 22), 2.6, 2.6);
        });
        if (c.ShowSecondHand)
            d.Hand(c.SecondAngle, 37, 0.7, RomanBlue, tail: 8);
        d.Circle(2.4, RomanBlue);
    }

    // ---- Squircle Sky ----
    private static readonly Brush SkyNight = VerticalGradient(("#0A1633", 0), ("#23406F", 1));
    private static readonly Brush SkyDawn = VerticalGradient(("#6D7FD1", 0), ("#F59A7A", 0.6), ("#FFD59A", 1));
    private static readonly Brush SkyDay = VerticalGradient(("#2F8CF0", 0), ("#8FD0FF", 1));
    private static readonly Brush SkyDusk = VerticalGradient(("#3B2A73", 0), ("#B24F86", 0.65), ("#FF8A6A", 1));
    private static readonly Brush SkyWhite = Brush("#FFFFFF"), SkyTicks = Brush("#BFFFFFFF"), SkyStar = Brush("#BFFFFFFF"), SkyGold = Brush("#FFD166");
    private static readonly Point[] Stars = [new(24, 22), new(74, 18), new(80, 72), new(20, 70), new(62, 84), new(35, 12)];

    private static void SquircleSky(Dial d, ClockViewModel c)
    {
        var sky = c.Phase switch
        {
            SkyPhase.Dawn => SkyDawn,
            SkyPhase.Day => SkyDay,
            SkyPhase.Dusk => SkyDusk,
            _ => SkyNight,
        };
        d.Context.DrawRoundedRectangle(sky, null, new Rect(3, 3, 94, 94), 24, 24);
        if (c.Phase == SkyPhase.Night)
        {
            foreach (var star in Stars)
                d.CircleAt(star.X, star.Y, 0.9, SkyStar);
        }
        d.Ticks(12, 40, 36, 2, SkyTicks);
        d.Hand(c.HourAngle, 22, 5.5, SkyWhite);
        d.Hand(c.MinuteAngle, 33, 3.8, SkyWhite);
        if (c.ShowSecondHand)
            d.Hand(c.SecondAngle, 35, 1.4, SkyGold, tail: 8);
        d.Circle(3, SkyWhite);
    }

    // ---- Mid-century ----
    private static readonly Brush MidFaceDay = Brush("#F7F6F2"), MidFaceNight = Brush("#2A2A2A"), MidRim = Brush("#CFCCC3");
    private static readonly Brush MidInkDay = Brush("#222222"), MidInkNight = Brush("#F2F0EA"), MidYellow = Brush("#F0B90B");

    private static void MidCentury(Dial d, ClockViewModel c)
    {
        var ink = c.IsNight ? MidInkNight : MidInkDay;
        d.Circle(47, c.IsNight ? MidFaceNight : MidFaceDay, MidRim, 1);
        d.Ticks(60, 46, 44, 0.6, ink);
        d.Numerals(ArabicNumerals, 36, 8.4, ink, SegoeSemibold);
        d.Hand(c.HourAngle, 22, 3, ink, cap: PenLineCap.Flat);
        d.Hand(c.MinuteAngle, 36, 2.2, ink, cap: PenLineCap.Flat);
        if (c.ShowSecondHand)
            d.Hand(c.SecondAngle, 38, 1.2, MidYellow, tail: 12);
        d.Circle(3, MidYellow);
    }

    // ---- Compact Strip ----
    private static readonly Brush StripFaceDay = Brush("#F2F4F7"), StripFaceNight = Brush("#2C3242");

    private static void CompactStrip(Dial d, ClockViewModel c)
    {
        // Drawn at ~30px, so no second hand and only four chunky ticks.
        var ink = c.IsNight ? MicaInkNight : MicaInkDay;
        d.Circle(47, c.IsNight ? StripFaceNight : StripFaceDay);
        d.Ticks(4, 42, 34, 5, ink);
        d.Hand(c.HourAngle, 22, 7, ink);
        d.Hand(c.MinuteAngle, 34, 5, ink);
    }

    // ---- Inset Digital ----
    private static readonly Brush InsetFaceDay = Brush("#F4F4F2"), InsetFaceNight = Brush("#2B2D33");
    private static readonly Brush InsetInkDay = Brush("#1F2023"), InsetInkNight = Brush("#F4F4F2");
    private static readonly Brush InsetDigitsDay = Brush("#6D7079"), InsetDigitsNight = Brush("#9EA3AD"), InsetOrange = Brush("#FF6B3D");

    private static void InsetDigital(Dial d, ClockViewModel c)
    {
        var ink = c.IsNight ? InsetInkNight : InsetInkDay;
        d.Circle(47, c.IsNight ? InsetFaceNight : InsetFaceDay);
        d.Ticks(12, 44, 39.5, 1.8, ink);
        d.Text(c.TimeText, 50, 69, c.Use24Hour ? 11 : 9, c.IsNight ? InsetDigitsNight : InsetDigitsDay, Bahnschrift);
        d.Hand(c.HourAngle, 22, 4.4, ink);
        d.Hand(c.MinuteAngle, 34, 2.8, ink);
        if (c.ShowSecondHand)
            d.Hand(c.SecondAngle, 37, 1.2, InsetOrange, tail: 8);
        d.Circle(3, InsetOrange);
    }
}
