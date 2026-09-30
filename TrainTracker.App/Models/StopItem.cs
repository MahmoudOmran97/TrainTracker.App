namespace TrainTracker.App.Models;

public class StopItem
{
    public StopItem(TripStop s, bool passed, bool isNear)
    {
        Name = Ui.Ar(s.StationName);
        TimeText = BuildTime(s);
        DotColor = isNear ? Ui.C("Signal") : passed ? Ui.C("Rule") : Ui.C("Ink");
        NameColor = passed && !isNear ? Ui.C("Muted") : Ui.C("Ink");
        NameFont = isNear ? "PlexBold" : "PlexMedium";
    }

    public string Name { get; }
    public string TimeText { get; }
    public Color DotColor { get; }
    public Color NameColor { get; }
    public string NameFont { get; }

    private static string BuildTime(TripStop s)
    {
        var arr = s.ScheduledArrival;
        var dep = s.ScheduledDeparture;
        if (arr is null) return Ui.Time(dep);
        if (dep is null || dep == arr) return Ui.Time(arr);
        return $"{Ui.Time(arr)} - {Ui.Time(dep)}";
    }
}
