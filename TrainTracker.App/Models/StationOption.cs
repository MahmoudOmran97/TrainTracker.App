namespace TrainTracker.App.Models;

/// <summary>محطة في قايمة الاقتراحات: الاسم المعروض بالإملاء الصح، والمحطة الأصلية للبحث.</summary>
public record StationOption(Station Station, string Display);
