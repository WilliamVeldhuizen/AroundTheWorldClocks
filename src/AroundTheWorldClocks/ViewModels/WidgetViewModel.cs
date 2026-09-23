using System.Collections.ObjectModel;
using System.Windows.Controls;
using AroundTheWorldClocks.Models;

namespace AroundTheWorldClocks.ViewModels;

public class WidgetViewModel : ObservableObject
{
    private Orientation _orientation;

    public ObservableCollection<ClockViewModel> Clocks { get; } = [];

    public Orientation Orientation { get => _orientation; private set => SetProperty(ref _orientation, value); }

    public void Apply(WidgetSettings settings)
    {
        Orientation = settings.Orientation == WidgetOrientation.Vertical ? Orientation.Vertical : Orientation.Horizontal;

        Clocks.Clear();
        foreach (var clock in settings.Clocks)
            Clocks.Add(new ClockViewModel(clock, settings));
    }

    public void Tick()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var clock in Clocks)
            clock.Update(now);
    }
}
