using System.Globalization;

namespace TrainTracker.App.Models;

/// <summary>الكارت زي ما بيتعرض: كل النصوص والألوان جاهزة عشان الـ XAML يفضل بسيط.</summary>
public class TripCardItem
{
    // صف النقط اللي بيمثل خط تقطيع التذكرة (بيتقص تلقائي على عرض الكارت)
    public static readonly string Perforation = string.Concat(Enumerable.Repeat("· ", 90));

    public TripCardItem(TripCard t)
    {
        TripId = t.TripId;
        TrainNumber = t.TrainNumber;
        TypeText = string.IsNullOrWhiteSpace(t.TrainType) ? "قطر" : t.TrainType!;
        From = t.From is null ? "—" : Ui.Ar(t.From);
        To = t.To is null ? "—" : Ui.Ar(t.To);
        DepartureText = Format(t.Departure);
        ArrivalText = Format(t.Arrival);
        HasLocation = t.LastLatitude is not null && t.LastLongitude is not null;
        IsRunning = t.Status == "Running";

        var delayed = t.DelayMinutes > 0;

        (StatusText, StatusFg, StatusBg) = t.Status switch
        {
            "Cancelled" => ("ملغي", C("Stop"), C("StopSoft")),
            "Completed" => ("وصل", C("Idle"), C("IdleSoft")),
            "Running" when delayed => ("متأخر", C("Late"), C("LateSoft")),
            "Running" => ("على الطريق", C("Go"), C("GoSoft")),
            _ => ("لسه ماتحركش", C("Idle"), C("IdleSoft")),
        };

        (StubText, StubColor) = t.Status switch
        {
            "Cancelled" => ("الرحلة دي اتلغت", C("Stop")),
            "Completed" => ("وصل المحطة الأخيرة", C("Muted")),
            _ when delayed => ($"متأخر {t.DelayMinutes} دقيقة", C("Late")),
            "Running" => ("ماشي في ميعاده", C("Go")),
            _ => ("", C("Muted")),
        };
    }

    public int TripId { get; }
    public string TrainNumber { get; }
    public string TypeText { get; }
    public string From { get; }
    public string To { get; }
    public string DepartureText { get; }
    public string ArrivalText { get; }
    public bool HasLocation { get; }
    public bool IsRunning { get; }

    public string StatusText { get; }
    public Color StatusFg { get; }
    public Color StatusBg { get; }
    public Brush StatusStroke => new SolidColorBrush(StatusFg);

    public string StubText { get; }
    public Color StubColor { get; }
    public bool HasStub => StubText.Length > 0;

    private static string Format(TimeOnly? t)
        => t is null ? "--:--" : t.Value.ToString("HH:mm", CultureInfo.InvariantCulture);

    // بنقرا اللون من الـ Colors.xaml عشان مفيش ألوان متكررة في الكود
    private static Color C(string key)
        => Application.Current!.Resources.TryGetValue(key, out var v) && v is Color c ? c : Colors.Black;
}
