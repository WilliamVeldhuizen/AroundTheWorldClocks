using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using AroundTheWorldClocks.Models;
using AroundTheWorldClocks.Services;
using AroundTheWorldClocks.ViewModels;

namespace AroundTheWorldClocks.Views;

/// <summary>
/// Borderless, transparent desktop widget showing one analog clock per configured city.
/// Drag with the left mouse button; right-click for settings and exit.
/// </summary>
public partial class WidgetWindow : Window
{
    private const double ScreenMargin = 16;

    private readonly SettingsStore _store;
    private readonly WidgetViewModel _viewModel = new();
    private readonly DispatcherTimer _timer = new();
    private WidgetSettings _settings;

    public WidgetWindow(SettingsStore store)
    {
        InitializeComponent();

        _store = store;
        _settings = store.Load();

        DataContext = _viewModel;
        BuildThemeMenu();
        ApplySettings();

        _timer.Tick += OnTimerTick;
        ContentRendered += OnContentRendered;
    }

    private void OnContentRendered(object? sender, EventArgs e)
    {
        // Positioned after the first render: only then is the final (SizeToContent) size known.
        RestorePosition();
        SizeChanged += (_, _) => KeepOnScreen();
        ScheduleNextTick();
        _timer.Start();
    }

    private void ApplySettings()
    {
        ApplyTheme(_settings.Theme);
        _viewModel.Apply(_settings);
        Topmost = _settings.AlwaysOnTop;
        AlwaysOnTopMenuItem.IsChecked = _settings.AlwaysOnTop;

        foreach (MenuItem item in ThemeMenuItem.Items)
            item.IsChecked = (WidgetTheme)item.Tag == _settings.Theme;
    }

    /// <summary>Swaps in Themes/{theme}.xaml, which supplies the PanelStyle and ClockItemTemplate resources.</summary>
    private void ApplyTheme(WidgetTheme theme)
    {
        var dictionary = new ResourceDictionary
        {
            Source = new Uri($"pack://application:,,,/Themes/{theme}.xaml", UriKind.Absolute),
        };
        Resources.MergedDictionaries.Clear();
        Resources.MergedDictionaries.Add(dictionary);
    }

    private void BuildThemeMenu()
    {
        foreach (var option in WidgetThemes.All)
        {
            var item = new MenuItem { Header = option.Name, Tag = option.Theme, IsCheckable = true };
            item.Click += OnThemeMenuClick;
            ThemeMenuItem.Items.Add(item);
        }
    }

    private void OnThemeMenuClick(object sender, RoutedEventArgs e)
    {
        _settings.Theme = (WidgetTheme)((MenuItem)sender).Tag;
        ApplySettings();
        SaveSettings();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        _viewModel.Tick();
        ScheduleNextTick();
    }

    /// <summary>Aligns ticks to the start of each second so the second hand doesn't drift.</summary>
    private void ScheduleNextTick()
    {
        int msToNextSecond = 1000 - DateTime.Now.Millisecond;
        _timer.Interval = TimeSpan.FromMilliseconds(Math.Max(msToNextSecond, 50));
    }

    private void RestorePosition()
    {
        if (_settings.Left is double left && _settings.Top is double top && IsOnScreen(left, top))
        {
            Left = left;
            Top = top;
            return;
        }

        // Default: top-right corner of the primary work area.
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - ActualWidth - ScreenMargin;
        Top = workArea.Top + ScreenMargin;
    }

    private bool IsOnScreen(double left, double top)
    {
        // Guard against a monitor that has since been disconnected.
        var right = SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth;
        var bottom = SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight;
        return left >= SystemParameters.VirtualScreenLeft - ActualWidth / 2
            && top >= SystemParameters.VirtualScreenTop
            && left < right - ActualWidth / 2
            && top < bottom - ActualHeight / 2;
    }

    /// <summary>When the widget grows (more clocks, bigger size) push it back inside the screen.</summary>
    private void KeepOnScreen()
    {
        var right = SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth;
        var bottom = SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight;
        Left = Math.Max(SystemParameters.VirtualScreenLeft, Math.Min(Left, right - ActualWidth));
        Top = Math.Max(SystemParameters.VirtualScreenTop, Math.Min(Top, bottom - ActualHeight));
    }

    private void SaveSettings()
    {
        _settings.Left = Left;
        _settings.Top = Top;
        _store.Save(_settings);
    }

    private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove(); // Blocks until the mouse button is released.
        SaveSettings();
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        var dialog = new SettingsWindow(_settings.Clone()) { Owner = this };
        if (dialog.ShowDialog() == true)
        {
            _settings = dialog.Result;
            ApplySettings();
            SaveSettings();
        }
    }

    private void OnAlwaysOnTopClick(object sender, RoutedEventArgs e)
    {
        _settings.AlwaysOnTop = AlwaysOnTopMenuItem.IsChecked;
        Topmost = _settings.AlwaysOnTop;
        SaveSettings();
    }

    private void OnExitClick(object sender, RoutedEventArgs e) => Close();

    protected override void OnClosing(CancelEventArgs e)
    {
        _timer.Stop();
        SaveSettings();
        base.OnClosing(e);
    }
}
