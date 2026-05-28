using FoodieApp.Models;
using FoodieApp.Services;
using FoodieApp.ViewModels;

namespace FoodieApp.Views;

public partial class MainPage : ContentPage
{
    private readonly RecipesViewModel _viewModel;
    private const double ShakeThreshold = 2.5;
    private bool _shakeReady = true;

    public MainPage(RecipesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadRecipesWithErrorHandling();
        StartAccelerometer();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopAccelerometer();
    }

    private void StartAccelerometer()
    {
        try
        {
            if (!Accelerometer.Default.IsSupported || Accelerometer.Default.IsMonitoring) return;
            Accelerometer.Default.ReadingChanged += OnAccelerometerReadingChanged;
            Accelerometer.Default.Start(SensorSpeed.Game);
        }
        catch (FeatureNotSupportedException) { }
    }

    private void StopAccelerometer()
    {
        try
        {
            if (!Accelerometer.Default.IsMonitoring) return;
            Accelerometer.Default.ReadingChanged -= OnAccelerometerReadingChanged;
            Accelerometer.Default.Stop();
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[MainPage] {ex.Message}"); }
    }

    private void OnAccelerometerReadingChanged(object? sender, AccelerometerChangedEventArgs e)
    {
        if (!_shakeReady) return;

        var d = e.Reading.Acceleration;
        double magnitude = Math.Sqrt(d.X * d.X + d.Y * d.Y + d.Z * d.Z);
        if (magnitude <= ShakeThreshold) return;

        _shakeReady = false;
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var recipe = _viewModel.GetRandomRecipe();
            if (recipe != null)
            {
                try { Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(500)); }
                catch (FeatureNotSupportedException) { }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[MainPage] {ex.Message}"); }

                await Shell.Current.GoToAsync($"RecipeDetailPage?recipeId={recipe.Id}");
            }

            await Task.Delay(2000);
            _shakeReady = true;
        });
    }

    private async void OnRecipeSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not Recipe recipe) return;
        ((CollectionView)sender).SelectedItem = null;
        await Shell.Current.GoToAsync($"RecipeDetailPage?recipeId={recipe.Id}");
    }

    private async Task LoadRecipesWithErrorHandling()
    {
        try   { await _viewModel.LoadRecipesAsync(); }
        catch (HttpRequestException) { await ErrorHelper.ShowNetworkErrorAsync(this); }
        catch (Exception ex)         { await ErrorHelper.ShowUnexpectedErrorAsync(this, ex); }
    }
}
