using FoodieApp.ViewModels;
namespace FoodieApp.Views;
public partial class ScanPage : ContentPage
{
    public ScanPage(ScanViewModel viewModel)
    {
        InitializeComponent(); BindingContext = viewModel;
    }
}