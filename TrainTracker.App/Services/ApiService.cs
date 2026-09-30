using System.Net.Http.Json;
using System.Text.Json;
using TrainTracker.App.Models;

namespace TrainTracker.App.Services;

public class ApiService
{
    // السيرفر المنشور. للتجربة على الـ API المحلي:
    //   إيميوليتر أندرويد: http://10.0.2.2:5279   |   موبايل حقيقي: http://<IP-جهازك>:5279 (وفعّل usesCleartextTraffic)
    public const string BaseUrl = "https://traintracker.runasp.net";

    private readonly HttpClient _http = new()
    {
        BaseAddress = new Uri(BaseUrl),
        Timeout = TimeSpan.FromSeconds(60)
    };

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private List<Station>? _stations;

    public async Task<List<TripCard>> GetTodayTripsAsync(CancellationToken ct = default)
        => await _http.GetFromJsonAsync<List<TripCard>>("/api/trips/today", Json, ct) ?? new();

    /// <summary>كل المحطات — بتتحمّل مرة واحدة وتتخزن (400+ محطة).</summary>
    public async Task<List<Station>> GetStationsAsync(CancellationToken ct = default)
    {
        if (_stations is not null) return _stations;
        _stations = await _http.GetFromJsonAsync<List<Station>>("/api/stations", Json, ct) ?? new();
        return _stations;
    }

    public async Task<List<TripSearchResult>> SearchAsync(int fromStationId, int toStationId, CancellationToken ct = default)
        => await _http.GetFromJsonAsync<List<TripSearchResult>>(
               $"/api/trips/search?fromStationId={fromStationId}&toStationId={toStationId}", Json, ct) ?? new();

    public Task<TripDetails?> GetTripAsync(int tripId, CancellationToken ct = default)
        => _http.GetFromJsonAsync<TripDetails>($"/api/trips/{tripId}", Json, ct);

    public async Task<bool> ReportPositionAsync(int tripId, double lat, double lon, double? speedKmh, CancellationToken ct = default)
    {
        using var resp = await _http.PostAsJsonAsync($"/api/trips/{tripId}/position",
            new PositionReport(lat, lon, speedKmh, UserId), Json, ct);
        return resp.IsSuccessStatusCode;
    }

    /// <summary>معرّف مجهول للجهاز (لحد ما نعمل تسجيل دخول).</summary>
    public static Guid UserId
    {
        get
        {
            if (Guid.TryParse(Preferences.Default.Get("user_id", ""), out var id)) return id;
            id = Guid.NewGuid();
            Preferences.Default.Set("user_id", id.ToString());
            return id;
        }
    }
}
