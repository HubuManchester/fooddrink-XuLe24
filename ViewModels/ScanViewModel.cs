using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace FoodieApp.ViewModels;
public partial class ScanViewModel : BaseViewModel
{
    [ObservableProperty] private ImageSource? _capturedImage;
    [ObservableProperty] private string _statusMessage = "Tap the button below to scan a food item or barcode.";
    [ObservableProperty] private bool _hasImage;
    public ScanViewModel() { Title = "Scan Food"; }
    [RelayCommand]
    public async Task CapturePhotoAsync()
    {
        if (IsBusy) return; IsBusy = true; StatusMessage = "Opening camera...";
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported) { StatusMessage = "Camera is not supported on this device."; return; }
            var photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions { Title = "Take a photo of your food" });
            if (photo == null) { StatusMessage = "Tap the button below to scan a food item or barcode."; return; }
            var stream = await photo.OpenReadAsync();
            CapturedImage = ImageSource.FromStream(() => stream);
            HasImage = true; StatusMessage = "Great shot! Identify this ingredient in your recipes.";
            HapticFeedback.Perform(HapticFeedbackType.Click);
        }
        catch (PermissionException) { StatusMessage = "Camera permission denied. Please enable it in Settings."; }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[ScanViewModel] {ex}"); StatusMessage = "Something went wrong. Please try again."; }
        finally { IsBusy = false; }
    }
    [RelayCommand]
    public async Task PickPhotoAsync()
    {
        if (IsBusy) return; IsBusy = true; StatusMessage = "Opening gallery...";
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();
            if (photo == null) { StatusMessage = "Tap the button below to scan a food item or barcode."; return; }
            var stream = await photo.OpenReadAsync();
            CapturedImage = ImageSource.FromStream(() => stream);
            HasImage = true; StatusMessage = "Photo loaded from gallery.";
            HapticFeedback.Perform(HapticFeedbackType.Click);
        }
        catch (PermissionException) { StatusMessage = "Gallery permission denied. Please enable it in Settings."; }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[ScanViewModel] {ex}"); StatusMessage = "Something went wrong. Please try again."; }
        finally { IsBusy = false; }
    }
    [RelayCommand]
    public void ClearPhoto() { CapturedImage = null; HasImage = false; StatusMessage = "Tap the button below to scan a food item or barcode."; }
}