using Microsoft.Extensions.Logging;
using TrainTracker.App.Pages;
using TrainTracker.App.Services;

namespace TrainTracker.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("IBMPlexSansArabic-Regular.ttf", "Plex");
                fonts.AddFont("IBMPlexSansArabic-Medium.ttf", "PlexMedium");
                fonts.AddFont("IBMPlexSansArabic-Bold.ttf", "PlexBold");
                fonts.AddFont("ReemKufi-Bold.ttf", "Kufi");
            });

        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddTransient<HomePage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
