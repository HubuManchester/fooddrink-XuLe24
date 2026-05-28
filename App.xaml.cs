using FoodieApp.Services;

namespace FoodieApp;

public partial class App : Application
{
    private readonly AppShell _shell;
    private readonly SettingsService _settingsService;

    public App(AppShell shell, SettingsService settingsService)
    {
        InitializeComponent();
        _shell = shell;
        _settingsService = settingsService;
        UserAppTheme = settingsService.IsDarkMode ? AppTheme.Dark : AppTheme.Light;
        settingsService.ApplyFontSize(this);
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(_shell);
        window.Created += (_, _) => _settingsService.ApplyFontSize(this);
        return window;
    }
}
