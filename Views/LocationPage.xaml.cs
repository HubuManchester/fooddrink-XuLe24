using FoodieApp.Services;
using FoodieApp.ViewModels;
namespace FoodieApp.Views;
public partial class LocationPage : ContentPage
{
    private readonly LocationViewModel _viewModel;
    public LocationPage(LocationViewModel viewModel)
    {
        InitializeComponent(); _viewModel = viewModel; BindingContext = _viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try   { await _viewModel.GetLocationAsync(); }
        catch (Exception ex) { await ErrorHelper.ShowUnexpectedErrorAsync(this, ex); }
    }
}