namespace FoodieApp.Services;
public static class ErrorHelper
{
    public static Task ShowErrorAsync(Page page, string title, string message) => page.DisplayAlert(title, message, "OK");
    public static Task ShowNetworkErrorAsync(Page page) => ShowErrorAsync(page, "Network Error", "Unable to reach the server. Please check your internet connection and try again.");
    public static Task ShowPermissionErrorAsync(Page page, string feature) => ShowErrorAsync(page, "Permission Required", $"Access to {feature} was denied. Please enable it in your device settings.");
    public static Task ShowFeatureNotSupportedAsync(Page page, string feature) => ShowErrorAsync(page, "Not Supported", $"{feature} is not available on this device.");
    public static Task ShowValidationErrorAsync(Page page, string message) => ShowErrorAsync(page, "Invalid Input", message);
    public static Task ShowUnexpectedErrorAsync(Page page, Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"[FoodieApp] Unexpected error: {ex}");
        return ShowErrorAsync(page, "Something Went Wrong", "An unexpected error occurred. Please try again.");
    }
}