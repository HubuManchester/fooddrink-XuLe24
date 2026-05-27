using CommunityToolkit.Maui;
using FoodieApp.Services;
using FoodieApp.ViewModels;
using FoodieApp.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoodieApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf",  "OpenSansRegular");
                fonts.AddFont("OpenSans-SemiBold.ttf", "OpenSansSemiBold");
            });

        builder.Services.AddHttpClient<RecipeService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(15);
        });

        builder.Services.AddSingleton<FavouritesService>();
        builder.Services.AddSingleton<SettingsService>();

        builder.Services.AddTransient<RecipesViewModel>();
        builder.Services.AddTransient<RecipeDetailViewModel>();
        builder.Services.AddTransient<ScanViewModel>();
        builder.Services.AddTransient<LocationViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<RecipeDetailPage>();
        builder.Services.AddTransient<ScanPage>();
        builder.Services.AddTransient<LocationPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<HelpPage>();

        builder.Services.AddSingleton<AppShell>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}