using FoodieApp.Services;
using FoodieApp.ViewModels;

namespace FoodieApp.Views;

public partial class RecipeDetailPage : ContentPage
{
    private readonly RecipeDetailViewModel _viewModel;

    public RecipeDetailPage(RecipeDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadRecipeCommand.ExecuteAsync(null);
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        await _viewModel.StopReadingCommand.ExecuteAsync(null);
    }

    private async void OnHelpClicked(object? sender, EventArgs e) =>
        await Shell.Current.GoToAsync(nameof(HelpPage));
}
