using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoodieApp.Services;

namespace FoodieApp.ViewModels;

/// <summary>View model for the GPS location page.</summary>
public partial class LocationViewModel : BaseViewModel
{
    [ObservableProperty] private string _locationText = "Tap the button to get your location.";
    [ObservableProperty] private string _latitudeText = "--";
    [ObservableProperty] private string _longitudeText = "--";
    [ObservableProperty] private bool _hasLocation;

    public LocationViewModel() => Title = "Nearby";

    /// <summary>Requests the device's current GPS location.</summary>
    [RelayCommand]
    public async Task GetLocationAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        LocationText = "Obtaining location...";

        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location is null)
            {
                LocationText = "Could not determine location. Please try again.";
                return;
            }

            LatitudeText = location.Latitude.ToString("F4");
            LongitudeText = location.Longitude.ToString("F4");
            HasLocation = true;
            LocationText = $"Lat: {location.Latitude:F4}, Lon: {location.Longitude:F4}";

            try
            {
                var placemarks = await Geocoding.Default.GetPlacemarksAsync(
                    location.Latitude, location.Longitude);
                var place = placemarks?.FirstOrDefault();

                if (place is not null)
                {
                    LocationText = $"{place.Thoroughfare}, {place.Locality}, {place.CountryName}"
                        .Trim(',', ' ');
                }
            }
            catch (Exception ex)
            {
                // GPS succeeded; geocoding often fails on Windows/emulators and is non-critical.
                System.Diagnostics.Debug.WriteLine($"[LocationViewModel] Geocoding skipped: {ex.Message}");
            }

            HapticFeedback.Perform(HapticFeedbackType.Click);
        }
        catch (FeatureNotSupportedException)
        {
            LocationText = "GPS is not supported on this device.";
            if (ErrorHelper.GetCurrentPage() is Page page)
                await ErrorHelper.ShowFeatureNotSupportedAsync(page, "GPS");
        }
        catch (PermissionException)
        {
            LocationText = "Location permission denied. Please enable it in Settings.";
            if (ErrorHelper.GetCurrentPage() is Page page)
                await ErrorHelper.ShowPermissionErrorWithSettingsAsync(page, "Location");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[LocationViewModel] {ex}");
            LocationText = "Failed to get location. Please try again.";
            if (ErrorHelper.GetCurrentPage() is Page page)
                await ErrorHelper.ShowUnexpectedErrorAsync(page, ex);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
