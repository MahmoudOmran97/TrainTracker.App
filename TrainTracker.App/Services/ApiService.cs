using System.Net.Http.Json;
using System.Text.Json;
using TrainTracker.App.Models;

namespace TrainTracker.App.Services;

public class ApiService
{
    // إيميوليتر أندرويد = 10.0.2.2 ، موبايل حقيقي = IP جهازك (مثال: http://192.168.1.10:5279)
    public const string BaseUrl = "https://traintracker.runasp.net";

    private readonly HttpClient _http = new()
    {
        BaseAddress = new Uri(BaseUrl),
        Timeout = TimeSpan.FromSeconds(20)
    };

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public async Task<List<TripCard>> GetTodayTripsAsync(CancellationToken ct = default)
        => await _http.GetFromJsonAsync<List<TripCard>>("/api/trips/today", Json, ct) ?? new();
}
