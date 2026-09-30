using TrainTracker.App.Models;
using TrainTracker.App.Services;

namespace TrainTracker.App.Pages;

public partial class HomePage : ContentPage
{
    private readonly ApiService _api;
    private bool _loaded;

    public HomePage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_loaded) await LoadAsync(showSpinner: true);
    }

    private async void OnRefreshing(object? sender, EventArgs e)
    {
        await LoadAsync(showSpinner: false);
        Refresh.IsRefreshing = false;
    }

    private async void OnRetry(object? sender, EventArgs e) => await LoadAsync(showSpinner: true);

    private async Task LoadAsync(bool showSpinner)
    {
        ErrorView.IsVisible = false;
        if (showSpinner) { Spinner.IsVisible = true; Spinner.IsRunning = true; }

        try
        {
            var trips = await _api.GetTodayTripsAsync();
            var items = trips
                .OrderBy(t => t.Departure ?? TimeOnly.MaxValue)
                .Select(t => new TripCardItem(t))
                .ToList();

            TripsList.ItemsSource = items;
            _loaded = true;

            var running = items.Count(i => i.IsRunning);
            SummaryLabel.Text = running > 0
                ? $"{items.Count} رحلة النهارده، و{running} منهم على الطريق"
                : $"{items.Count} رحلة النهارده";
        }
        catch (Exception)
        {
            TripsList.ItemsSource = null;
            SummaryLabel.Text = "مفيش اتصال";
            ErrorDetail.Text = $"شغّل الـ API وتأكد إن العنوان صح:\n{ApiService.BaseUrl}";
            ErrorView.IsVisible = true;
        }
        finally
        {
            Spinner.IsRunning = false;
            Spinner.IsVisible = false;
        }
    }
}
