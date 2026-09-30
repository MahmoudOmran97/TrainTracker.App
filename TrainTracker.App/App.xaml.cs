namespace TrainTracker.App;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Light; // التصميم مظبوط على الوضع الفاتح بس
    }

    protected override Window CreateWindow(IActivationState? activationState)
        => new Window(new AppShell());
}
