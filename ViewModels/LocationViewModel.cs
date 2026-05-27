using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
namespace FoodieApp.ViewModels;
public partial class LocationViewModel : BaseViewModel
{
    [ObservableProperty] private string _locationText = "Tap the button to get your location.";
    [ObservableProperty] private string _latitudeText = "--";
    [ObservableProperty] private string _longitudeText = "--";
    [ObservableProperty] private bool _hasLocation;
    public LocationViewModel() { Title = "Nearby"; }
    [RelayCommand]
    public async Task GetLocationAsync()
    {
        if (IsBusy) return; IsBusy = true; LocationText = "Obtaining location...";
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);
            if (location == null) { LocationText = "Could not determine location. Please try again."; return; }
            LatitudeText = location.Latitude.ToString("F4");
            LongitudeText = location.Longitude.ToString("F4");
            HasLocation = true;
            var placemarks = await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude);
            var place = placemarks?.FirstOrDefault();
            LocationText = place != null
                ? $"{place.Thoroughfare}, {place.Locality}, {place.CountryName}".Trim(',', ' ')
                : $"Lat: {location.Latitude:F4}, Lon: {location.Longitude:F4}";
            HapticFeedback.Perform(HapticFeedbackType.Click);
        }
        catch (FeatureNotSupportedException) { LocationText = "GPS is not supported on this device."; }
        catch (PermissionException) { LocationText = "Location permission denied. Please enable it in Settings."; }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[LocationViewModel] {ex}"); LocationText = "Failed to get location. Please try again."; }
        finally { IsBusy = false; }
    }
}