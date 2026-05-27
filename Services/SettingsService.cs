namespace FoodieApp.Services;
public class SettingsService
{
    private const string KeyFontSize = "font_size";
    private const string KeyDarkMode = "dark_mode";
    public const double MinFontSize = 12.0;
    public const double MaxFontSize = 24.0;
    public const double DefaultFontSize = 16.0;
    public double FontSize
    {
        get => Preferences.Get(KeyFontSize, DefaultFontSize);
        set => Preferences.Set(KeyFontSize, Math.Clamp(value, MinFontSize, MaxFontSize));
    }
    public bool IsDarkMode
    {
        get => Preferences.Get(KeyDarkMode, false);
        set => Preferences.Set(KeyDarkMode, value);
    }
    public void ApplyTheme()
    {
        if (Application.Current is null) return;
        Application.Current.UserAppTheme = IsDarkMode ? AppTheme.Dark : AppTheme.Light;
    }
}