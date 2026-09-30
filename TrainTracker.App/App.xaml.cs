namespace TrainTracker.App;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        UserAppTheme = AppTheme.Light; // التصميم مظبوط على الوضع الفاتح بس
        MainPage = new AppShell();
    }
}
