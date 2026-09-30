namespace TrainTracker.App.Models;

public class SearchResultItem
{
    public SearchResultItem(TripSearchResult r)
    {
        TripId = r.TripId;
        TitleText = $"{(string.IsNullOrWhiteSpace(r.TrainType) ? "قطر" : r.TrainType)} {r.TrainNumber}";
        TimesText = $"{Ui.Time(r.Departure)}  ←  {Ui.Time(r.Arrival)}";
        MetaText = r.DelayMinutes > 0 && r.Status == "Running"
            ? $"{r.StopsCount} محطة · متأخر {r.DelayMinutes} دقيقة"
            : $"{r.StopsCount} محطة";
        (StatusText, StatusFg, StatusBg) = Ui.Status(r.Status, r.DelayMinutes);
    }

    public int TripId { get; }
    public string TitleText { get; }
    public string TimesText { get; }
    public string MetaText { get; }
    public string StatusText { get; }
    public Color StatusFg { get; }
    public Color StatusBg { get; }
}
