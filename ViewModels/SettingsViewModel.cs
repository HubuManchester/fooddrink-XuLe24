using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodieApp.Services;
namespace FoodieApp.ViewModels;
public partial class SettingsViewModel : BaseViewModel
{
    private readonly SettingsService _settingsService;
    [ObservableProperty] private string _fontValidationMessage = string.Empty;
    [ObservableProperty] private bool _isFontValidationVisible;
    [ObservableProperty] private double _fontSize;
    [ObservableProperty] private bool _isDarkMode;
    public SettingsViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService; Title = "Settings";
        _fontSize = _settingsService.FontSize; _isDarkMode = _settingsService.IsDarkMode;
    }
    [RelayCommand]
    public void SaveFontSize()
    {
        if (FontSize < SettingsService.MinFontSize || FontSize > SettingsService.MaxFontSize)
        { FontValidationMessage = $"Font size must be between {SettingsService.MinFontSize} and {SettingsService.MaxFontSize}."; IsFontValidationVisible = true; return; }
        IsFontValidationVisible = false; _settingsService.FontSize = FontSize;
        HapticFeedback.Perform(HapticFeedbackType.Click);
    }
    [RelayCommand]
    public void ToggleDarkMode() { _settingsService.IsDarkMode = IsDarkMode; _settingsService.ApplyTheme(); }
    [RelayCommand]
    public void ResetDefaults()
    {
        FontSize = SettingsService.DefaultFontSize; IsDarkMode = false;
        _settingsService.FontSize = FontSize; _settingsService.IsDarkMode = IsDarkMode;
        _settingsService.ApplyTheme(); IsFontValidationVisible = false;
        HapticFeedback.Perform(HapticFeedbackType.LongPress);
    }
}