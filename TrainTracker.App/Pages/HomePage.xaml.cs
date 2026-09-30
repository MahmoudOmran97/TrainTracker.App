using TrainTracker.App.Models;
using TrainTracker.App.Services;

namespace TrainTracker.App.Pages;

public partial class HomePage : ContentPage
{
    private readonly ApiService _api;
    private bool _loaded;
    private List<TripCardItem> _items = new();
    private string _summary = "";

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

    private async void OnCardTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is TripCardItem item)
            await Shell.Current.GoToAsync($"trip?id={item.TripId}");
    }

    private void OnTrainSearchChanged(object? sender, TextChangedEventArgs e) => ApplyFilter();

    // فلتر محلي على رحلات النهارده برقم القطر (بيقبل أرقام عربي وإنجليزي)
    private void ApplyFilter()
    {
        var q = Ui.Digits(TrainSearch.Text).Trim();
        var list = q.Length == 0
            ? _items
            : _items.Where(i => i.TrainNumber.Contains(q, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(i => i.TrainNumber == q ? 0 : i.TrainNumber.StartsWith(q) ? 1 : 2)
                    .ToList();

        TripsList.ItemsSource = list;
        SummaryLabel.Text = q.Length == 0 ? _summary : $"{list.Count} نتيجة لرقم {q}";
        EmptyTitle.Text = q.Length == 0 ? "مفيش رحلات النهارده" : $"مفيش قطر رقمه {q}";
        EmptyHint.Text = q.Length == 0 ? "اسحب الشاشة لتحت عشان تحدّث." : "راجع الرقم أو امسح البحث.";
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

            _items = items;
            _loaded = true;

            var running = items.Count(i => i.IsRunning);
            _summary = running > 0
                ? $"{items.Count} رحلة النهارده، و{running} منهم على الطريق"
                : $"{items.Count} رحلة النهارده";
            ApplyFilter();
        }
        catch (Exception ex)
        {
            _items = new();
            TripsList.ItemsSource = null;
            SummaryLabel.Text = "مفيش اتصال";
            ErrorDetail.Text = $"{ex.GetType().Name}: {ex.Message}\n{ApiService.BaseUrl}";
            ErrorView.IsVisible = true;
        }
        finally
        {
            Spinner.IsRunning = false;
            Spinner.IsVisible = false;
        }
    }
}
