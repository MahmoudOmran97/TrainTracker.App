using TrainTracker.App.Services;

namespace TrainTracker.App.Pages;

public partial class ProfilePage : ContentPage
{
    public ProfilePage()
    {
        InitializeComponent();
        NameEntry.Text = Preferences.Default.Get("display_name", "");
        IdLabel.Text = $"المعرّف: {ApiService.UserId.ToString()[..8]}";
        VersionLabel.Text = $"الإصدار: {AppInfo.Current.VersionString}";
        ServerLabel.Text = $"السيرفر: {ApiService.BaseUrl}";
    }

    private async void OnSave(object? sender, EventArgs e)
    {
        Preferences.Default.Set("display_name", NameEntry.Text?.Trim() ?? "");
        SavedLabel.IsVisible = true;
        await Task.Delay(1800);
        SavedLabel.IsVisible = false;
    }
}
