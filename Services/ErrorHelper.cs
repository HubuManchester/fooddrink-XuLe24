namespace FoodieApp.Services;

/// <summary>Centralised user-friendly error dialogs and safe async execution.</summary>
public static class ErrorHelper
{
    /// <summary>Shows a simple alert dialog.</summary>
    public static Task ShowErrorAsync(Page page, string title, string message) =>
        page.DisplayAlert(title, message, "OK");

    /// <summary>Shows a network connectivity error message.</summary>
    public static Task ShowNetworkErrorAsync(Page page) =>
        ShowErrorAsync(page, "Network Error",
            "Unable to reach the server. Please check your internet connection and try again.");

    /// <summary>Shows a permission denied message with an option to open device settings.</summary>
    public static async Task ShowPermissionErrorWithSettingsAsync(Page page, string feature)
    {
        var openSettings = await page.DisplayAlert(
            "Permission Required",
            $"Access to {feature} was denied. Please enable it in your device settings.",
            "Open Settings",
            "Cancel");

        if (openSettings)
            AppInfo.ShowSettingsUI();
    }

    /// <summary>Shows a permission denied message without opening settings.</summary>
    public static Task ShowPermissionErrorAsync(Page page, string feature) =>
        ShowErrorAsync(page, "Permission Required",
            $"Access to {feature} was denied. Please enable it in your device settings.");

    /// <summary>Shows a feature-not-supported message.</summary>
    public static Task ShowFeatureNotSupportedAsync(Page page, string feature) =>
        ShowErrorAsync(page, "Not Supported", $"{feature} is not available on this device.");

    /// <summary>Shows a validation error message.</summary>
    public static Task ShowValidationErrorAsync(Page page, string message) =>
        ShowErrorAsync(page, "Invalid Input", message);

    /// <summary>Shows a generic unexpected error without exposing technical details.</summary>
    public static Task ShowUnexpectedErrorAsync(Page page, Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"[FoodieApp] Unexpected error: {ex}");
        return ShowErrorAsync(page, "Something Went Wrong",
            "An unexpected error occurred. Please try again.");
    }

    /// <summary>Gets the current Shell page for displaying alerts from ViewModels.</summary>
    public static Page? GetCurrentPage() => Shell.Current?.CurrentPage;

    /// <summary>Runs an async action and shows user-friendly alerts on failure.</summary>
    public static async Task RunSafeAsync(Page page, Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (HttpRequestException)
        {
            await ShowNetworkErrorAsync(page);
        }
        catch (PermissionException)
        {
            await ShowPermissionErrorWithSettingsAsync(page, "the requested feature");
        }
        catch (FeatureNotSupportedException ex)
        {
            await ShowFeatureNotSupportedAsync(page, ex.Message);
        }
        catch (Exception ex)
        {
            await ShowUnexpectedErrorAsync(page, ex);
        }
    }

    /// <summary>Runs an async function and shows user-friendly alerts on failure.</summary>
    public static async Task<T?> RunSafeAsync<T>(Page page, Func<Task<T>> action)
    {
        try
        {
            return await action();
        }
        catch (HttpRequestException)
        {
            await ShowNetworkErrorAsync(page);
            return default;
        }
        catch (Exception ex)
        {
            await ShowUnexpectedErrorAsync(page, ex);
            return default;
        }
    }
}
