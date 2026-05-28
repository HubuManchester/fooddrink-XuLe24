using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodieApp.Services;

namespace FoodieApp.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly SettingsService _settingsService;

    public IReadOnlyList<string> FontSizeOptions { get; } =
        ["Normal", "Large", "Extra Large"];

    [ObservableProperty] private bool _isDarkMode;
    [ObservableProperty] private string _selectedFontSizeOption = "Normal";
    [ObservableProperty] private double _previewFontSize = 16;

    public SettingsViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;
        Title = "Settings";
        _isDarkMode = settingsService.IsDarkMode;
        _selectedFontSizeOption = SettingsService.GetDisplayName(settingsService.FontSizePreset);
        _previewFontSize = settingsService.GetFontSizeValue();
    }

    partial void OnIsDarkModeChanged(bool value)
    {
        _settingsService.IsDarkMode = value;
        _settingsService.ApplyTheme();
    }

    partial void OnSelectedFontSizeOptionChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;

        var preset = SettingsService.FromDisplayName(value);
        _settingsService.FontSizePreset = preset;
        PreviewFontSize = _settingsService.GetFontSizeValue();
        _settingsService.ApplyFontSize();
        HapticFeedback.Perform(HapticFeedbackType.Click);
    }

    [RelayCommand]
    public void ResetDefaults()
    {
        IsDarkMode = false;
        SelectedFontSizeOption = "Normal";
        _settingsService.IsDarkMode = false;
        _settingsService.FontSizePreset = FontSizePreset.Normal;
        _settingsService.ApplyAll();
        HapticFeedback.Perform(HapticFeedbackType.LongPress);
    }
}
