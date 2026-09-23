using System.Windows;
using AroundTheWorldClocks.Services;
using AroundTheWorldClocks.Views;

namespace AroundTheWorldClocks;

public partial class App : Application
{
    private Mutex? _singleInstanceMutex;

    private void OnStartup(object sender, StartupEventArgs e)
    {
        // Only one widget instance at a time.
        _singleInstanceMutex = new Mutex(true, "AroundTheWorldClocks.SingleInstance", out bool isFirstInstance);
        if (!isFirstInstance)
        {
            Shutdown();
            return;
        }

        var window = new WidgetWindow(new SettingsStore());
        MainWindow = window;
        window.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _singleInstanceMutex?.Dispose();
        base.OnExit(e);
    }
}
