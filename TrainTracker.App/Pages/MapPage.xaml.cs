using System.Text.Json;
using TrainTracker.App.Models;
using TrainTracker.App.Services;

namespace TrainTracker.App.Pages;

[QueryProperty(nameof(TripIdText), "id")]
public partial class MapPage : ContentPage
{
    private readonly ApiService _api;
    private int _tripId;
    private string _payload = "null";
    private bool _loaded;

    public MapPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        Web.Navigated += OnWebNavigated;
    }

    public string TripIdText
    {
        set => _tripId = int.TryParse(value, out var id) ? id : 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_loaded || _tripId <= 0) return;

        try
        {
            var trip = await _api.GetTripAsync(_tripId);
            if (trip is null) { InfoLabel.Text = "الرحلة مش موجودة"; return; }

            var stops = trip.Stops.Where(s => Ui.HasCoords(s.Latitude, s.Longitude)).ToList();
            double[]? train = trip.LastLatitude is double la && trip.LastLongitude is double lo
                ? new[] { lo, la } : null;

            // GeoJSON بيستخدم (lon, lat)
            _payload = JsonSerializer.Serialize(new
            {
                route = stops.Select(s => new[] { s.Longitude, s.Latitude }),
                stops = stops.Select(s => new { n = Ui.Ar(s.StationName), c = new[] { s.Longitude, s.Latitude } }),
                train
            });

            InfoLabel.Text = $"قطر {trip.TrainNumber}" + (train is null ? " — لسه مفيش موقع متبلّغ" : "");
            _loaded = true;
            Web.Source = new HtmlWebViewSource { Html = MapHtml, BaseUrl = ApiService.BaseUrl + "/" };
        }
        catch (Exception ex)
        {
            InfoLabel.Text = $"مفيش اتصال: {ex.Message}";
        }
    }

    private async void OnWebNavigated(object? sender, WebNavigatedEventArgs e)
    {
        if (e.Result != WebNavigationResult.Success) return;
        await Web.EvaluateJavaScriptAsync($"setData({_payload})");
    }

    private async void OnBack(object? sender, EventArgs e) => await Shell.Current.GoToAsync("..");

    // خريطة MapLibre GL JS على بلاطات OpenFreeMap. محتاجة إنترنت (الـ JS بييجي من CDN).
    private const string MapHtml = """
<!DOCTYPE html>
<html><head><meta charset="utf-8"/>
<meta name="viewport" content="width=device-width, initial-scale=1"/>
<link href="https://cdn.jsdelivr.net/npm/maplibre-gl@4.7.1/dist/maplibre-gl.css" rel="stylesheet"/>
<script src="https://cdn.jsdelivr.net/npm/maplibre-gl@4.7.1/dist/maplibre-gl.js"></script>
<style>html,body,#map{margin:0;height:100%;width:100%}</style></head>
<body><div id="map"></div>
<script>
var ready = false, pending = null, done = false;
// بدون الإضافة دي الحروف العربية بتظهر مقطّعة ومعكوسة
maplibregl.setRTLTextPlugin('https://unpkg.com/@mapbox/mapbox-gl-rtl-text@0.3.0/dist/mapbox-gl-rtl-text.js', true);
var map = new maplibregl.Map({
  container: 'map',
  style: 'https://tiles.openfreemap.org/styles/liberty',
  center: [31.2, 30.0], zoom: 6
});
map.addControl(new maplibregl.NavigationControl());

function fc(features) { return { type: 'FeatureCollection', features: features }; }

function apply(d) {
  var line = fc(d.route.length >= 2 ? [{ type: 'Feature', geometry: { type: 'LineString', coordinates: d.route } }] : []);
  var pts = fc(d.stops.map(function (s) { return { type: 'Feature', properties: { n: s.n }, geometry: { type: 'Point', coordinates: s.c } }; }));
  var tr = fc(d.train ? [{ type: 'Feature', geometry: { type: 'Point', coordinates: d.train } }] : []);

  if (!done) {
    map.addSource('route', { type: 'geojson', data: line });
    map.addLayer({ id: 'route', type: 'line', source: 'route', paint: { 'line-color': '#12263A', 'line-width': 4 } });
    map.addSource('stops', { type: 'geojson', data: pts });
    map.addLayer({ id: 'stops', type: 'circle', source: 'stops',
      paint: { 'circle-radius': 5, 'circle-color': '#FBFCFC', 'circle-stroke-color': '#12263A', 'circle-stroke-width': 2 } });
    map.addSource('train', { type: 'geojson', data: tr });
    map.addLayer({ id: 'train', type: 'circle', source: 'train',
      paint: { 'circle-radius': 10, 'circle-color': '#F2A900', 'circle-stroke-color': '#FFFFFF', 'circle-stroke-width': 3 } });
    map.on('click', 'stops', function (e) {
      new maplibregl.Popup().setLngLat(e.lngLat).setText(e.features[0].properties.n).addTo(map);
    });
    done = true;
  } else {
    map.getSource('route').setData(line);
    map.getSource('stops').setData(pts);
    map.getSource('train').setData(tr);
  }

  var b = new maplibregl.LngLatBounds();
  d.route.forEach(function (c) { b.extend(c); });
  if (d.train) b.extend(d.train);
  if (!b.isEmpty()) map.fitBounds(b, { padding: 48, duration: 0, maxZoom: 13 });
}

map.on('load', function () {
  // ستايل Liberty بيكتب الاسم اللاتيني + العربي، فبنخليه عربي بس (ولو ناقص بنرجع للاسم الأصلي)
  map.getStyle().layers.forEach(function (l) {
    if (l.type !== 'symbol' || !l.layout || !l.layout['text-field']) return;
    if (JSON.stringify(l.layout['text-field']).indexOf('"name') < 0) return;
    map.setLayoutProperty(l.id, 'text-field', ['coalesce', ['get', 'name:ar'], ['get', 'name']]);
  });
  ready = true; if (pending) apply(pending);
});
function setData(d) { if (d === null) return; if (ready) apply(d); else pending = d; }
</script></body></html>
""";
}
