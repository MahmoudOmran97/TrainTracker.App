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

    public async Task<List<TripCard>> GetTodayTripsAsync(CancellationToken ct = default)
        => await _http.GetFromJsonAsync<List<TripCard>>("/api/trips/today", Json, ct) ?? new();
}
