using TrainTracker.App.Models;
using TrainTracker.App.Services;

namespace TrainTracker.App.Pages;

[QueryProperty(nameof(TripIdText), "id")]
public partial class TripDetailsPage : ContentPage
{
    private readonly ApiService _api;
    private int _tripId;

    public TripDetailsPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    public string TripIdText
    {
        set => _tripId = int.TryParse(value, out var id) ? id : 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_tripId > 0) await LoadAsync();
    }

    private async Task LoadAsync()
    {
        ErrorLabel.IsVisible = false;
        Spinner.IsVisible = Spinner.IsRunning = true;
        try
        {
            var trip = await _api.GetTripAsync(_tripId);
            if (trip is null) { ShowError("الرحلة دي مش موجودة."); return; }

            TypeLabel.Text = string.IsNullOrWhiteSpace(trip.TrainType) ? "قطر" : trip.TrainType;
            NumberLabel.Text = trip.TrainNumber;
            var first = trip.Stops.FirstOrDefault();
            var last = trip.Stops.LastOrDefault();
            RouteLabel.Text = first is null ? "" : $"{first.StationName}  ←  {last!.StationName}";

            // أقرب محطة للموقع الأخير (بنتجاهل المحطات اللي من غير إحداثيات)
            var nearIndex = -1;
            double nearKm = 0;
            if (trip.LastLatitude is double lat && trip.LastLongitude is double lon)
            {
                var best = double.MaxValue;
                for (var i = 0; i < trip.Stops.Count; i++)
                {
                    var s = trip.Stops[i];
                    if (!Ui.HasCoords(s.Latitude, s.Longitude)) continue;
                    var d = Ui.Km(lat, lon, s.Latitude, s.Longitude);
                    if (d < best) { best = d; nearIndex = i; }
                }
                nearKm = best;
            }

            (WhereLabel.Text, WhereSub.Text) = DescribeWhere(trip, nearIndex, nearKm);

            BindableLayout.SetItemsSource(StopsLayout,
                trip.Stops.Select((s, i) => new StopItem(s, passed: nearIndex >= 0 && i < nearIndex, isNear: i == nearIndex)).ToList());
        }
        catch (Exception ex)
        {
            ShowError($"مفيش اتصال\n{ex.GetType().Name}: {ex.Message}");
        }
        finally
        {
            Spinner.IsRunning = Spinner.IsVisible = false;
        }
    }

    private static (string Main, string Sub) DescribeWhere(TripDetails trip, int nearIndex, double nearKm)
    {
        var delay = trip.DelayMinutes > 0 ? $"متأخر {trip.DelayMinutes} دقيقة" : "";
        if (trip.Status == "Cancelled") return ("الرحلة دي اتلغت", "");
        if (trip.Status == "Completed") return ("القطر وصل المحطة الأخيرة", "");

        if (trip.LastLatitude is null)
            return ("لسه مفيش بلاغ عن موقعه", "لو إنت في القطر دوس \"ابعت موقعي\" وساعد الناس.");

        if (nearIndex < 0)
            return ("في بلاغ بموقعه", "إحداثيات المحطات لسه ناقصة فمش قادرين نحدد أقرب محطة.");

        var name = trip.Stops[nearIndex].StationName;
        var main = nearKm < 1 ? $"عند محطة {name}" : $"قريب من محطة {name} (حوالي {Math.Round(nearKm)} كم)";
        return (main, delay);
    }

    private void ShowError(string text) { ErrorLabel.Text = text; ErrorLabel.IsVisible = true; }

    private async void OnBack(object? sender, EventArgs e) => await Shell.Current.GoToAsync("..");

    private async void OnMap(object? sender, EventArgs e) => await Shell.Current.GoToAsync($"map?id={_tripId}");

    private async void OnReport(object? sender, EventArgs e)
    {
        ReportBtn.IsEnabled = false;
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                await DisplayAlert("الموقع", "محتاجين إذن الموقع عشان تبلّغ بمكان القطر.", "حاضر");
                return;
            }

            var loc = await Geolocation.Default.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(15)));
            if (loc is null)
            {
                await DisplayAlert("الموقع", "مقدرتش أجيب موقعك. جرّب تاني وإنت في مكان مفتوح.", "حاضر");
                return;
            }

            double? kmh = loc.Speed is double ms ? ms * 3.6 : null; // م/ث → كم/س
            var ok = await _api.ReportPositionAsync(_tripId, loc.Latitude, loc.Longitude, kmh);
            await DisplayAlert("البلاغ", ok ? "شكرًا! موقعك اتبعت." : "البلاغ ماوصلش، جرّب تاني.", "تمام");
            if (ok) await LoadAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("مشكلة", $"{ex.GetType().Name}: {ex.Message}", "تمام");
        }
        finally
        {
            ReportBtn.IsEnabled = true;
        }
    }
}
