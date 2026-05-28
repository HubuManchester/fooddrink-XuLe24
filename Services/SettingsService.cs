namespace FoodieApp.Services;

public enum FontSizePreset
{
    Normal,
    Large,
    ExtraLarge
}

public class SettingsService
{
    private const string KeyFontSizePreset = "font_size_preset";
    private const string KeyFontSizeLegacy = "font_size";
    private const string KeyDarkMode = "dark_mode";

    public FontSizePreset FontSizePreset
    {
        get
        {
            var stored = Preferences.Get(KeyFontSizePreset, string.Empty);
            if (!string.IsNullOrEmpty(stored) &&
                Enum.TryParse<FontSizePreset>(stored, out var preset))
                return preset;

            var legacy = Preferences.Get(KeyFontSizeLegacy, 16.0);
            return legacy switch
            {
                >= 22 => FontSizePreset.ExtraLarge,
                >= 18 => FontSizePreset.Large,
                _ => FontSizePreset.Normal
            };
        }
        set => Preferences.Set(KeyFontSizePreset, value.ToString());
    }

    public bool IsDarkMode
    {
        get => Preferences.Get(KeyDarkMode, false);
        set => Preferences.Set(KeyDarkMode, value);
    }

    public double GetFontSizeValue() => FontSizePreset switch
    {
        FontSizePreset.Large      => 20,
        FontSizePreset.ExtraLarge => 24,
        _                         => 16
    };

    public static string GetDisplayName(FontSizePreset preset) => preset switch
    {
        FontSizePreset.Large      => "Large",
        FontSizePreset.ExtraLarge => "Extra Large",
        _                         => "Normal"
    };

    public static FontSizePreset FromDisplayName(string displayName) => displayName switch
    {
        "Large"       => FontSizePreset.Large,
        "Extra Large" => FontSizePreset.ExtraLarge,
        _             => FontSizePreset.Normal
    };

    public void ApplyTheme()
    {
        if (Application.Current is null) return;
        Application.Current.UserAppTheme = IsDarkMode ? AppTheme.Dark : AppTheme.Light;
    }

    public void ApplyFontSize(Application? app = null)
    {
        var resources = (app ?? Application.Current)?.Resources;
        if (resources is null) return;

        var body = GetFontSizeValue();
        resources["BaseFontSize"]     = body;
        resources["HeadlineFontSize"] = body + 6;
        resources["TitleFontSize"]    = body + 2;
        resources["CaptionFontSize"]  = Math.Max(12, body - 2);
    }

    public void ApplyAll(Application? app = null)
    {
        ApplyTheme();
        ApplyFontSize(app);
    }
}
