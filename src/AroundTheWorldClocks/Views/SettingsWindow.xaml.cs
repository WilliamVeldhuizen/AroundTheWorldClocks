using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using AroundTheWorldClocks.Models;
using AroundTheWorldClocks.Services;

namespace AroundTheWorldClocks.Views;

public partial class SettingsWindow : Window
{
    private readonly ObservableCollection<CityClock> _clocks;
    private string _suggestedCityName = "";
    private CityClock? _editingClock;

    public SettingsWindow(WidgetSettings settings)
    {
        InitializeComponent();

        Result = settings;
        _clocks = new ObservableCollection<CityClock>(settings.Clocks);
        ClockList.ItemsSource = _clocks;
        TimeZoneCombo.ItemsSource = TimeZoneInfo.GetSystemTimeZones();

        ThemeCombo.ItemsSource = WidgetThemes.All;
        ThemeCombo.SelectedValue = settings.Theme;
        SizeSlider.Value = settings.ClockSize;
        Use24HourBox.IsChecked = settings.Use24HourFormat;
        SecondHandBox.IsChecked = settings.ShowSecondHand;
        VerticalBox.IsChecked = settings.Orientation == WidgetOrientation.Vertical;
        AlwaysOnTopBox.IsChecked = settings.AlwaysOnTop;
        AutoStartBox.IsChecked = AutoStartService.IsEnabled;
    }

    /// <summary>The edited settings; valid after the dialog returns true.</summary>
    public WidgetSettings Result { get; }

    private void OnTimeZoneSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TimeZoneCombo.SelectedItem is not TimeZoneInfo zone)
            return;

        // Prefill the city name from the zone's display name, e.g. "(UTC+01:00) Amsterdam, Berlin, ..." -> "Amsterdam",
        // unless the user already typed their own name.
        if (CityNameBox.Text.Length == 0 || CityNameBox.Text == _suggestedCityName)
        {
            _suggestedCityName = SuggestCityName(zone);
            CityNameBox.Text = _suggestedCityName;
        }
    }

    private static string SuggestCityName(TimeZoneInfo zone)
    {
        var name = zone.DisplayName;
        int closeParen = name.IndexOf(')');
        if (closeParen >= 0)
            name = name[(closeParen + 1)..];

        int comma = name.IndexOf(',');
        if (comma >= 0)
            name = name[..comma];

        return name.Trim();
    }

    private void OnAddClick(object sender, RoutedEventArgs e)
    {
        if (TimeZoneCombo.SelectedItem is not TimeZoneInfo zone)
        {
            MessageBox.Show(this, "Select a time zone first.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var name = CityNameBox.Text.Trim();
        var clock = new CityClock(name.Length > 0 ? name : SuggestCityName(zone), zone.Id);
        _clocks.Add(clock);
        ClockList.SelectedItem = clock;

        CityNameBox.Text = "";
        _suggestedCityName = "";
    }

    private void OnEditClick(object sender, RoutedEventArgs e)
    {
        if (ClockList.SelectedItem is not CityClock clock)
            return;

        _editingClock = clock;
        var zone = FindTimeZone(clock.TimeZoneId);
        TimeZoneCombo.SelectedItem = zone;
        CityNameBox.Text = clock.DisplayName;
        // Keep following the zone's suggested name only if the clock still uses it.
        _suggestedCityName = zone is not null && clock.DisplayName == SuggestCityName(zone) ? clock.DisplayName : "";

        ClockFormGroup.Header = "Edit clock";
        AddButton.Visibility = Visibility.Collapsed;
        EditButtons.Visibility = Visibility.Visible;
        CityNameBox.Focus();
        CityNameBox.SelectAll();
    }

    private TimeZoneInfo? FindTimeZone(string id)
    {
        var zones = (IEnumerable<TimeZoneInfo>)TimeZoneCombo.ItemsSource;
        var zone = zones.FirstOrDefault(z => z.Id == id);
        if (zone is null && TimeZoneInfo.TryConvertIanaIdToWindowsId(id, out var windowsId))
            zone = zones.FirstOrDefault(z => z.Id == windowsId);
        return zone;
    }

    private void OnSaveEditClick(object sender, RoutedEventArgs e)
    {
        if (_editingClock is null)
            return;

        if (TimeZoneCombo.SelectedItem is not TimeZoneInfo zone)
        {
            MessageBox.Show(this, "Select a time zone first.", Title, MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        int index = _clocks.IndexOf(_editingClock);
        if (index >= 0)
        {
            var name = CityNameBox.Text.Trim();
            var clock = new CityClock(name.Length > 0 ? name : SuggestCityName(zone), zone.Id);
            _clocks[index] = clock;
            ClockList.SelectedItem = clock;
        }

        EndEdit();
    }

    private void OnCancelEditClick(object sender, RoutedEventArgs e) => EndEdit();

    private void EndEdit()
    {
        _editingClock = null;
        TimeZoneCombo.SelectedItem = null;
        CityNameBox.Text = "";
        _suggestedCityName = "";

        ClockFormGroup.Header = "Add clock";
        AddButton.Visibility = Visibility.Visible;
        EditButtons.Visibility = Visibility.Collapsed;
    }

    private void OnRemoveClick(object sender, RoutedEventArgs e)
    {
        int index = ClockList.SelectedIndex;
        if (index < 0)
            return;

        if (_clocks[index] == _editingClock)
            EndEdit();

        _clocks.RemoveAt(index);
        ClockList.SelectedIndex = Math.Min(index, _clocks.Count - 1);
    }

    private void OnMoveUpClick(object sender, RoutedEventArgs e) => MoveSelected(-1);

    private void OnMoveDownClick(object sender, RoutedEventArgs e) => MoveSelected(+1);

    private void MoveSelected(int delta)
    {
        int index = ClockList.SelectedIndex;
        int target = index + delta;
        if (index < 0 || target < 0 || target >= _clocks.Count)
            return;

        _clocks.Move(index, target);
        ClockList.SelectedIndex = target;
    }

    private void OnOkClick(object sender, RoutedEventArgs e)
    {
        Result.Clocks = _clocks.ToList();
        Result.Theme = ThemeCombo.SelectedValue is WidgetTheme theme ? theme : WidgetTheme.MicaGlass;
        Result.ClockSize = SizeSlider.Value;
        Result.Use24HourFormat = Use24HourBox.IsChecked == true;
        Result.ShowSecondHand = SecondHandBox.IsChecked == true;
        Result.Orientation = VerticalBox.IsChecked == true ? WidgetOrientation.Vertical : WidgetOrientation.Horizontal;
        Result.AlwaysOnTop = AlwaysOnTopBox.IsChecked == true;

        bool autoStart = AutoStartBox.IsChecked == true;
        if (autoStart != AutoStartService.IsEnabled)
            AutoStartService.SetEnabled(autoStart);

        DialogResult = true;
    }
}
