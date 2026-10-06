using Arkana.Data;
using Arkana.Models;
using Arkana.Services;

namespace Arkana.Pages;

public partial class ProfilePage : ContentPage
{
    readonly AppDbContext db;

    public ProfilePage(AppDbContext db)
    {
        InitializeComponent();
        this.db = db;
    }

    // cada vez que se abre el perfil, se consultan los datos
    protected override void OnAppearing()
    {
        base.OnAppearing();

        User usuario = Session.CurrentUser;

        if (usuario == null)
        {
            return;
        }

        NameLabel.Text = usuario.Name;
        UsernameLabel.Text = "@" + usuario.Username;

        // consultas a la base de datos con LINQ
        PostsLabel.Text = db.Posts.Count(p => p.UserId == usuario.Id).ToString();
        FollowersLabel.Text = db.Follows.Count(f => f.FollowingId == usuario.Id).ToString();
        FollowingLabel.Text = db.Follows.Count(f => f.FollowerId == usuario.Id).ToString();
    }
}