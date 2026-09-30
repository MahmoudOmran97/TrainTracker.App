using TrainTracker.App.Pages;

namespace TrainTracker.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute("trip", typeof(TripDetailsPage));
        Routing.RegisterRoute("map", typeof(MapPage));
    }
}
