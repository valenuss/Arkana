using Arkana.Data;
using Arkana.Helpers;
using Arkana.Services;

namespace Arkana.Pages;

public partial class ProfilePage : ContentPage
{
    readonly AppDbContext db;
    readonly FollowService follows;

    public ProfilePage(AppDbContext db, FollowService follows)
    {
        InitializeComponent();
        this.db = db;
        this.follows = follows;
    }

    // cada vez que se abre el perfil, se consultan los datos
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (Session.CurrentUser is not { } user)
        {
            return;
        }

        ProfileAvatar.Initial = user.Username.ToInitial();
        NameLabel.Text = user.Name;
        UsernameLabel.Text = "@" + user.Username;

        // mis publicaciones, las más nuevas primero
        var posts = db.Posts
            .Where(p => p.UserId == user.Id)
            .OrderByDescending(p => p.Date)
            .ToList();

        PostsLabel.Text = posts.Count.ToString();
        FollowersLabel.Text = follows.CountFollowers(user.Id).ToString();
        FollowingLabel.Text = follows.CountFollowing(user.Id).ToString();

        EmptyLabel.IsVisible = posts.Count == 0;

        MyPostsList.ItemsSource = posts
            .Select(p => new ProfilePostItem(
                p.Content,
                p.Date.ToString("dd/MM/yyyy"),
                p.Likes,
                string.IsNullOrEmpty(p.ImagePath) ? null : ImageSource.FromFile(p.ImagePath)))
            .ToList();
    }

    // cerrar sesión y volver al login
    private async void Logout_Clicked(object? sender, EventArgs e) =>
        await AuthNavigation.SignOutAsync();
}


// datos de cada publicación en el perfil
public record ProfilePostItem(string Content, string DateText, int Likes, ImageSource? Photo)
{
    public bool HasImage => Photo is not null;
    public bool IsTextOnly => Photo is null;
}