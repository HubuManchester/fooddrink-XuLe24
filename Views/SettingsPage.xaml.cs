using FoodieApp.ViewModels;
namespace FoodieApp.Views;
public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent(); _viewModel = viewModel; BindingContext = _viewModel;
    }
    private void OnFontSizeDragCompleted(object sender, EventArgs e) => _viewModel.SaveFontSize();
    private void OnDarkModeToggled(object sender, ToggledEventArgs e) => _viewModel.ToggleDarkMode();
}