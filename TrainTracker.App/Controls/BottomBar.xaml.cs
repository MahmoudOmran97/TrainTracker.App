using TrainTracker.App.Models;
using Shape = Microsoft.Maui.Controls.Shapes.Path;

namespace TrainTracker.App.Controls;

/// <summary>شريط التنقل السفلي. الاستخدام: &lt;c:BottomBar Active="home" /&gt; (home | search | profile)</summary>
public partial class BottomBar : ContentView
{
    public static readonly BindableProperty ActiveProperty = BindableProperty.Create(
        nameof(Active), typeof(string), typeof(BottomBar), "home",
        propertyChanged: (b, _, _) => ((BottomBar)b).Apply());

    public string Active
    {
        get => (string)GetValue(ActiveProperty);
        set => SetValue(ActiveProperty, value);
    }

    public BottomBar()
    {
        InitializeComponent();
        Apply();
    }

    private void Apply()
    {
        Set(0, "home", HomeItem, HomeIcon, HomeText);
        Set(1, "search", SearchItem, SearchIcon, SearchText);
        Set(2, "profile", ProfileItem, ProfileIcon, ProfileText);
    }

    private void Set(int column, string key, Border item, Shape icon, Label text)
    {
        var on = Active == key;
        Bar.ColumnDefinitions[column].Width = new GridLength(on ? 2.4 : 1, GridUnitType.Star);
        item.BackgroundColor = on ? Ui.C("Signal") : Colors.Transparent;
        icon.Stroke = new SolidColorBrush(on ? Ui.C("Ink") : Color.FromArgb("#8FA3B5"));
        text.IsVisible = on;
    }

    private async void OnTap(object? sender, TappedEventArgs e)
    {
        var route = e.Parameter as string;
        if (string.IsNullOrEmpty(route) || route == Active) return;
        await Shell.Current.GoToAsync($"//{route}");
    }
}
