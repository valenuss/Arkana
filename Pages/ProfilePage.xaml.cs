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

        PostsLabel.Text = db.Posts.Count(p => p.UserId == user.Id).ToString();
        FollowersLabel.Text = follows.CountFollowers(user.Id).ToString();
        FollowingLabel.Text = follows.CountFollowing(user.Id).ToString();
    }
}