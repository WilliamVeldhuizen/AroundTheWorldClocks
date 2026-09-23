using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace AroundTheWorldClocks.Rendering;

/// <summary>
/// Drawing helpers for a clock dial in a 100×100 coordinate space centred on (50, 50).
/// Angles are in degrees clockwise from twelve o'clock.
/// </summary>
internal sealed class Dial(DrawingContext dc, double pixelsPerDip)
{
    public static readonly Point Center = new(50, 50);

    public DrawingContext Context => dc;

    public void Circle(double radius, Brush? fill, Brush? stroke = null, double thickness = 0) =>
        dc.DrawEllipse(fill, stroke is null ? null : new Pen(stroke, thickness), Center, radius, radius);

    public void CircleAt(double x, double y, double radius, Brush? fill, Brush? stroke = null, double thickness = 0) =>
        dc.DrawEllipse(fill, stroke is null ? null : new Pen(stroke, thickness), new Point(x, y), radius, radius);

    /// <summary>A straight hand from <paramref name="tail"/> behind the centre to <paramref name="length"/> in front of it.</summary>
    public void Hand(double angle, double length, double width, Brush brush, double tail = 0, PenLineCap cap = PenLineCap.Round)
    {
        var pen = new Pen(brush, width) { StartLineCap = cap, EndLineCap = cap };
        dc.PushTransform(new RotateTransform(angle, 50, 50));
        dc.DrawLine(pen, new Point(50, 50 + tail), new Point(50, 50 - length));
        dc.Pop();
    }

    /// <summary>Draws arbitrary content rotated to <paramref name="angle"/>, e.g. a hand made of several shapes.</summary>
    public void Rotated(double angle, Action<DrawingContext> draw)
    {
        dc.PushTransform(new RotateTransform(angle, 50, 50));
        draw(dc);
        dc.Pop();
    }

    /// <summary><paramref name="count"/> evenly spaced ticks between radius <paramref name="outer"/> and <paramref name="inner"/>.</summary>
    public void Ticks(int count, double outer, double inner, double width, Brush brush, PenLineCap cap = PenLineCap.Flat)
    {
        var pen = new Pen(brush, width) { StartLineCap = cap, EndLineCap = cap };
        for (int i = 0; i < count; i++)
        {
            dc.PushTransform(new RotateTransform(i * 360.0 / count, 50, 50));
            dc.DrawLine(pen, new Point(50, 50 - outer), new Point(50, 50 - inner));
            dc.Pop();
        }
    }

    /// <summary>Labels spaced evenly around the dial, the first one at twelve o'clock.</summary>
    public void Numerals(IReadOnlyList<string> labels, double radius, double size, Brush brush, Typeface typeface)
    {
        for (int i = 0; i < labels.Count; i++)
        {
            double rad = i * 2 * Math.PI / labels.Count;
            Text(labels[i], 50 + radius * Math.Sin(rad), 50 - radius * Math.Cos(rad), size, brush, typeface);
        }
    }

    /// <summary>Text centred on (x, y).</summary>
    public void Text(string text, double x, double y, double size, Brush brush, Typeface typeface)
    {
        var formatted = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight,
            typeface, size, brush, pixelsPerDip);
        dc.DrawText(formatted, new Point(x - formatted.Width / 2, y - formatted.Height / 2));
    }

    public static SolidColorBrush Brush(string hex)
    {
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        brush.Freeze();
        return brush;
    }

    public static LinearGradientBrush VerticalGradient(params (string Color, double Offset)[] stops)
    {
        var brush = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(0, 1) };
        foreach (var (color, offset) in stops)
            brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(color), offset));
        brush.Freeze();
        return brush;
    }
}
