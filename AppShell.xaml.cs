using FoodieApp.Views;
namespace FoodieApp;
public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(RecipeDetailPage), typeof(RecipeDetailPage));
        Routing.RegisterRoute(nameof(HelpPage),         typeof(HelpPage));
    }
}