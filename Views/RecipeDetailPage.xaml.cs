using FoodieApp.Services;
using FoodieApp.ViewModels;
namespace FoodieApp.Views;
public partial class RecipeDetailPage : ContentPage
{
    private readonly RecipeDetailViewModel _viewModel;
    public RecipeDetailPage(RecipeDetailViewModel viewModel)
    {
        InitializeComponent(); _viewModel = viewModel; BindingContext = _viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try   { await _viewModel.LoadRecipeAsync(); }
        catch (HttpRequestException) { await ErrorHelper.ShowNetworkErrorAsync(this); }
        catch (Exception ex)         { await ErrorHelper.ShowUnexpectedErrorAsync(this, ex); }
    }
    protected override void OnDisappearing() { base.OnDisappearing(); _viewModel.StopSpeaking(); }
    private void OnPreviousStepClicked(object sender, EventArgs e) => _viewModel.PreviousStep();
    private void OnNextStepClicked(object sender, EventArgs e)     => _viewModel.NextStep();
    private async void OnSpeakClicked(object sender, EventArgs e)
    {
        try   { await _viewModel.SpeakCurrentStepAsync(); }
        catch (FeatureNotSupportedException) { await ErrorHelper.ShowFeatureNotSupportedAsync(this, "Text-to-Speech"); }
        catch (Exception ex)                 { await ErrorHelper.ShowUnexpectedErrorAsync(this, ex); }
    }
    private void OnStopSpeakingClicked(object sender, EventArgs e) => _viewModel.StopSpeaking();
    private void OnFavouriteClicked(object? sender, EventArgs e)   => _viewModel.ToggleFavourite();
    private async void OnHelpClicked(object? sender, EventArgs e)  =>
        await Shell.Current.GoToAsync(nameof(HelpPage));
}