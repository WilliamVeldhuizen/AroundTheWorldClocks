using System.Windows;
using System.Windows.Media;
using AroundTheWorldClocks.Rendering;
using AroundTheWorldClocks.ViewModels;

namespace AroundTheWorldClocks.Controls;

/// <summary>
/// Analog dial for the <see cref="ClockViewModel"/> in its DataContext, drawn in the view model's theme.
/// Redraws whenever the view model updates (once per second).
/// </summary>
public class ClockFace : FrameworkElement
{
    private ClockViewModel? _clock;

    public ClockFace()
    {
        DataContextChanged += (_, _) => Attach(DataContext as ClockViewModel);
        Loaded += (_, _) => Attach(DataContext as ClockViewModel);
        Unloaded += (_, _) => Attach(null);
    }

    private void Attach(ClockViewModel? clock)
    {
        if (_clock is not null)
            _clock.Updated -= OnClockUpdated;

        _clock = clock;

        if (_clock is not null)
            _clock.Updated += OnClockUpdated;

        InvalidateVisual();
    }

    private void OnClockUpdated(object? sender, EventArgs e) => InvalidateVisual();

    protected override void OnRender(DrawingContext dc)
    {
        double size = Math.Min(ActualWidth, ActualHeight);
        if (_clock is null || size <= 0)
            return;

        // Center a square 100×100 dial in the available space.
        dc.PushTransform(new TranslateTransform((ActualWidth - size) / 2, (ActualHeight - size) / 2));
        dc.PushTransform(new ScaleTransform(size / 100, size / 100));
        ClockRenderer.Render(new Dial(dc, VisualTreeHelper.GetDpi(this).PixelsPerDip), _clock);
        dc.Pop();
        dc.Pop();
    }
}
