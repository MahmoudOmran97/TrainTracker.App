using TrainTracker.App.Models;
using TrainTracker.App.Services;

namespace TrainTracker.App.Pages;

public partial class SearchPage : ContentPage
{
    private readonly ApiService _api;
    private List<Station> _all = new();
    private Station? _from, _to;
    private Entry? _active;
    private bool _suppress;

    public SearchPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_all.Count > 0) return;
        try
        {
            _all = (await _api.GetStationsAsync()).OrderBy(s => s.NameAr).ToList();
        }
        catch (Exception ex)
        {
            ShowHint($"مش قادر أحمّل المحطات\n{ex.GetType().Name}: {ex.Message}");
        }
    }

    private void OnEntryFocused(object? sender, FocusEventArgs e)
    {
        _active = (Entry)sender!;
        ShowSuggestions(_active.Text);
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_suppress) return;
        var entry = (Entry)sender!;
        _active = entry;
        if (entry == FromEntry) _from = null; else _to = null;
        ShowSuggestions(e.NewTextValue);
    }

    private void ShowSuggestions(string? text)
    {
        Results.IsVisible = false;
        var q = Norm(text);
        if (q.Length == 0)
        {
            Suggestions.IsVisible = false;
            ShowHint("اكتب اسم المحطة وهتظهرلك اقتراحات.");
            return;
        }

        var list = _all
            .Where(s => Norm(s.NameAr).Contains(q))
            .GroupBy(s => s.NameAr)
            .Select(g => g.First())
            .OrderBy(s => Norm(s.NameAr).StartsWith(q) ? 0 : 1)
            .ThenBy(s => s.NameAr)
            .Take(8)
            .ToList();

        Suggestions.ItemsSource = list.Select(s => new StationOption(s, Ui.Ar(s.NameAr))).ToList();
        Suggestions.IsVisible = list.Count > 0;
        if (list.Count > 0) Hint.IsVisible = false;
        else ShowHint("مفيش محطة بالاسم ده.");
    }

    private void OnSuggestionTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is not StationOption opt) return;
        var st = opt.Station;
        var entry = _active ?? FromEntry;

        _suppress = true;
        entry.Text = opt.Display;
        _suppress = false;

        if (entry == FromEntry) { _from = st; ToEntry.Focus(); }
        else { _to = st; entry.Unfocus(); }

        Suggestions.IsVisible = false;
        ShowHint("دوس \"دوّر\" عشان تشوف القطارات.");
    }

    private async void OnSearch(object? sender, EventArgs e)
    {
        // لو كتب الاسم كامل من غير ما يختار من الاقتراحات
        _from ??= _all.FirstOrDefault(s => Norm(s.NameAr) == Norm(FromEntry.Text));
        _to ??= _all.FirstOrDefault(s => Norm(s.NameAr) == Norm(ToEntry.Text));

        if (_from is null || _to is null) { ShowHint("اختار المحطتين من الاقتراحات الأول."); return; }
        if (_from.Id == _to.Id) { ShowHint("محطة المغادرة والوصول لازم يكونوا مختلفين."); return; }

        Suggestions.IsVisible = false;
        Results.IsVisible = false;
        Hint.IsVisible = false;
        Spinner.IsVisible = Spinner.IsRunning = true;
        try
        {
            var found = await _api.SearchAsync(_from.Id, _to.Id);
            if (found.Count == 0)
            {
                ShowHint("مفيش قطارات بين المحطتين دول النهارده.");
                return;
            }
            Results.ItemsSource = found
                .Select(r => new SearchResultItem(r))
                .ToList();
            Results.IsVisible = true;
        }
        catch (Exception ex)
        {
            ShowHint($"مفيش اتصال\n{ex.GetType().Name}: {ex.Message}");
        }
        finally
        {
            Spinner.IsRunning = Spinner.IsVisible = false;
        }
    }

    private async void OnResultTapped(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is SearchResultItem item)
            await Shell.Current.GoToAsync($"trip?id={item.TripId}");
    }

    private void ShowHint(string text) { Hint.Text = text; Hint.IsVisible = true; }

    // توحيد الهمزات والتاء المربوطة والياء عشان البحث يلقط "الاسكندرية" و"الإسكندرية"
    private static string Norm(string? s)
        => (s ?? "").Trim().Replace('أ', 'ا').Replace('إ', 'ا').Replace('آ', 'ا')
                    .Replace('ة', 'ه').Replace('ى', 'ي');
}
