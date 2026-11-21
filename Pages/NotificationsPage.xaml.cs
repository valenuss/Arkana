using Arkana.Services;

namespace Arkana.Pages;

public partial class NotificationsPage : ContentPage
{
    readonly AppDbContext db;

    public NotificationsPage(AppDbContext db)
    {
        InitializeComponent();
        this.db = db;
    }

    // cada vez que se abre, se arma la lista
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (Session.CurrentUser is not { } me)
        {
            return;
        }

        // comentarios que otras personas hicieron en mis publicaciones
        var myPostIds = db.Posts
            .Where(p => p.UserId == me.Id)
            .Select(p => p.Id)
            .ToList();

        var comments = db.Comments
            .Where(c => myPostIds.Contains(c.PostId) && c.UserId != me.Id)
            .ToList()
            .OrderByDescending(c => c.Date)
            .Select(c => new NotificationItem(
                Icons.ChatBubbleOutline,
                $"@{c.Username} comentó tu publicación: \"{c.Text}\"",
                c.Date.ToTimeAgo()));

        // personas que empezaron a seguirme
        var followers = db.Follows
            .Where(f => f.FollowingId == me.Id)
            .ToList()
            .Select(f => db.Users.Find(f.FollowerId))
            .Where(u => u is not null)
            .Select(u => new NotificationItem(
                Icons.PersonAdd,
                $"@{u!.Username} empezó a seguirte",
                ""));

        NotificationsList.ItemsSource = comments.Concat(followers).ToList();
    }

    private async void Back_Tapped(object? sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("..");
}


// datos de cada notificación tal como se muestra
public record NotificationItem(string Icon, string Message, string TimeAgo);