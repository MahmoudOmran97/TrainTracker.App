using System.Globalization;

namespace TrainTracker.App.Models;

/// <summary>حاجات مشتركة بين الصفحات: ألوان الحالة وتنسيق الوقت والمسافة.</summary>
public static class Ui
{
    public static Color C(string key)
        => Application.Current!.Resources.TryGetValue(key, out var v) && v is Color c ? c : Colors.Black;

    public static string Time(TimeOnly? t)
        => t is null ? "--:--" : t.Value.ToString("HH:mm", CultureInfo.InvariantCulture);

    public static (string Text, Color Fg, Color Bg) Status(string status, int delayMinutes) => status switch
    {
        "Cancelled" => ("ملغي", C("Stop"), C("StopSoft")),
        "Completed" => ("وصل", C("Idle"), C("IdleSoft")),
        "Running" when delayMinutes > 0 => ("متأخر", C("Late"), C("LateSoft")),
        "Running" => ("على الطريق", C("Go"), C("GoSoft")),
        _ => ("لسه ماتحركش", C("Idle"), C("IdleSoft")),
    };


    // أسماء المحطات اللي جاية من الاستيراد من غير همزات — بنصلحها وقت العرض بس
    private static readonly Dictionary<string, string> ArWords = new()
    {
        ["ابو"] = "أبو", ["ادفو"] = "إدفو", ["اسوان"] = "أسوان", ["اسيوط"] = "أسيوط",
        ["الاسكندرية"] = "الإسكندرية", ["الاقصر"] = "الأقصر", ["اسنا"] = "إسنا",
        ["ارمنت"] = "أرمنت", ["احمد"] = "أحمد",
    };

    /// <summary>اسم المحطة بالإملاء الصح (إدفو، أبو تشت...). ضيف كلمات في ArWords لو لقيت غيرها.</summary>
    public static string Ar(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return name ?? "";
        var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        for (var i = 0; i < parts.Length; i++)
            if (ArWords.TryGetValue(parts[i], out var fix)) parts[i] = fix;
        return string.Join(' ', parts);
    }

    /// <summary>يحوّل الأرقام العربية (٠-٩) والفارسية لأرقام إنجليزي عشان البحث برقم القطر.</summary>
    public static string Digits(string? s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        var chars = s.Select(c => c switch
        {
            >= '\u0660' and <= '\u0669' => (char)('0' + (c - '\u0660')),
            >= '\u06F0' and <= '\u06F9' => (char)('0' + (c - '\u06F0')),
            _ => c
        });
        return new string(chars.ToArray());
    }

    // المحطات اللي اتعملت من الاستيراد من غير إحداثيات بتبقى 0,0 — مش بنعتمد عليها
    public static bool HasCoords(double lat, double lon) => !(lat == 0 && lon == 0);

    public static double Km(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        double Rad(double d) => d * Math.PI / 180;
        var dLat = Rad(lat2 - lat1);
        var dLon = Rad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(Rad(lat1)) * Math.Cos(Rad(lat2)) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
