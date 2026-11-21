using Arkana.Services;

namespace Arkana.Pages;

public partial class NotificationsPage : ContentPage
{
    readonly NotificationService notifications;

    public NotificationsPage(NotificationService notifications)
    {
        InitializeComponent();
        this.notifications = notifications;
    }

    // cada vez que se abre, se arma la lista y se marcan como vistas
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (Session.CurrentUser is not { } me)
        {
            return;
        }

        NotificationsList.ItemsSource = notifications.GetItems(me.Id);
        notifications.MarkAllSeen(me.Id);
    }

    private async void Back_Tapped(object? sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("..");
}