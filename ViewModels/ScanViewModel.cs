using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodieApp.Services;

namespace FoodieApp.ViewModels;

/// <summary>View model for the camera and gallery scan page.</summary>
public partial class ScanViewModel : BaseViewModel
{
    [ObservableProperty] private ImageSource? _capturedImage;
    [ObservableProperty] private string _statusMessage = "Tap the button below to scan a food item or barcode.";
    [ObservableProperty] private bool _hasImage;

    public ScanViewModel() => Title = "Scan Food";

    /// <summary>Opens the device camera to capture a food photo.</summary>
    [RelayCommand]
    public async Task CapturePhotoAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        StatusMessage = "Opening camera...";

        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await ShowCameraUnavailableAsync(
                    "Camera is not supported on this device. Try Choose from Gallery instead.");
                return;
            }

            if (!await EnsureCameraPermissionAsync())
                return;

            var photo = await MediaPicker.Default.CapturePhotoAsync(
                new MediaPickerOptions { Title = "Take a photo of your food" });

            if (photo is null)
            {
                StatusMessage = "Tap the button below to scan a food item or barcode.";
                return;
            }

            await SetCapturedImageAsync(photo, "Great shot! Identify this ingredient in your recipes.");
        }
        catch (PermissionException)
        {
            StatusMessage = "Camera permission denied. Please enable it in Settings.";
            if (ErrorHelper.GetCurrentPage() is Page page)
                await ErrorHelper.ShowPermissionErrorWithSettingsAsync(page, "Camera");
        }
        catch (FeatureNotSupportedException)
        {
            await ShowCameraUnavailableAsync(
                "Camera is not available. On an emulator, enable Virtual Scene in Extended Controls, or use Choose from Gallery.");
        }
        catch (Exception ex) when (IsCameraUnavailable(ex))
        {
            System.Diagnostics.Debug.WriteLine($"[ScanViewModel] Camera unavailable: {ex}");
            await ShowCameraUnavailableAsync(
                "Camera could not be opened. On an Android emulator, open Extended Controls (⋯) → Camera → enable Virtual Scene, or tap Choose from Gallery.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ScanViewModel] {ex}");
            StatusMessage = "Something went wrong. Please try again.";
            if (ErrorHelper.GetCurrentPage() is Page page)
                await ErrorHelper.ShowUnexpectedErrorAsync(page, ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Opens the gallery to pick an existing food photo.</summary>
    [RelayCommand]
    public async Task PickPhotoAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        StatusMessage = "Opening gallery...";

        try
        {
            if (!await EnsurePhotosPermissionAsync())
                return;

            var photo = await MediaPicker.Default.PickPhotoAsync();
            if (photo is null)
            {
                StatusMessage = "Tap the button below to scan a food item or barcode.";
                return;
            }

            await SetCapturedImageAsync(photo, "Photo loaded from gallery.");
        }
        catch (PermissionException)
        {
            StatusMessage = "Gallery permission denied. Please enable it in Settings.";
            if (ErrorHelper.GetCurrentPage() is Page page)
                await ErrorHelper.ShowPermissionErrorWithSettingsAsync(page, "Photo Library");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ScanViewModel] {ex}");
            StatusMessage = "Something went wrong. Please try again.";
            if (ErrorHelper.GetCurrentPage() is Page page)
                await ErrorHelper.ShowUnexpectedErrorAsync(page, ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Clears the currently captured or selected photo.</summary>
    [RelayCommand]
    public void ClearPhoto()
    {
        CapturedImage = null;
        HasImage = false;
        StatusMessage = "Tap the button below to scan a food item or barcode.";
    }

    private static async Task<bool> EnsureCameraPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status == PermissionStatus.Granted)
            return true;

        status = await Permissions.RequestAsync<Permissions.Camera>();
        if (status == PermissionStatus.Granted)
            return true;

        if (ErrorHelper.GetCurrentPage() is Page page)
            await ErrorHelper.ShowPermissionErrorWithSettingsAsync(page, "Camera");

        return false;
    }

    private static async Task<bool> EnsurePhotosPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Photos>();
        if (status == PermissionStatus.Granted)
            return true;

        if (status == PermissionStatus.Denied)
        {
            status = await Permissions.RequestAsync<Permissions.Photos>();
            if (status == PermissionStatus.Granted)
                return true;
        }

        // Older Android versions may not require Photos permission for the picker.
        if (status is PermissionStatus.Granted or PermissionStatus.Unknown)
            return true;

        if (ErrorHelper.GetCurrentPage() is Page page)
            await ErrorHelper.ShowPermissionErrorWithSettingsAsync(page, "Photo Library");

        return false;
    }

    private async Task SetCapturedImageAsync(FileResult photo, string successMessage)
    {
        await using var stream = await photo.OpenReadAsync();
        var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        CapturedImage = ImageSource.FromStream(() => memoryStream);
        HasImage = true;
        StatusMessage = successMessage;
        HapticFeedback.Perform(HapticFeedbackType.Click);
    }

    private async Task ShowCameraUnavailableAsync(string message)
    {
        StatusMessage = message;
        if (ErrorHelper.GetCurrentPage() is Page page)
            await ErrorHelper.ShowErrorAsync(page, "Camera Unavailable", message);
    }

    private static bool IsCameraUnavailable(Exception ex)
    {
        var text = ex.ToString();
        return text.Contains("camera", StringComparison.OrdinalIgnoreCase)
            || text.Contains("resolve activity", StringComparison.OrdinalIgnoreCase)
            || text.Contains("No Activity found", StringComparison.OrdinalIgnoreCase)
            || text.Contains("Cannot find", StringComparison.OrdinalIgnoreCase);
    }
}
