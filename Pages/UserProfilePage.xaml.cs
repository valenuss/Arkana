using System.Collections.ObjectModel;
using Arkana.Data;
using Arkana.Helpers;
using Arkana.Models;
using Arkana.Services;

namespace Arkana.Pages;

public partial class UserProfilePage : ContentPage, IQueryAttributable
{
    readonly UserProfileData data;

    public UserProfilePage(AppDbContext db, FollowService follows)
    {
        InitializeComponent();

        data = new UserProfileData(db, follows);
        BindingContext = data;
    }

    // recibe el parámetro de la ruta: userprofile?id=3
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var id))
        {
            data.Load(Convert.ToInt32(id));
        }
    }

    // volver a la pantalla anterior
    private async void Back_Tapped(object? sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("..");

    // seguir o dejar de seguir
    private void Follow_Clicked(object? sender, EventArgs e) => data.ToggleFollow();
}


// datos del perfil de otro usuario
public class UserProfileData : BindingUtilObject
{
    readonly AppDbContext db;
    readonly FollowService follows;
    int userId;

    public UserProfileData(AppDbContext db, FollowService follows)
    {
        this.db = db;
        this.follows = follows;
    }

    string _name = "";
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    string _handle = "";
    public string Handle
    {
        get => _handle;
        set => SetProperty(ref _handle, value);
    }

    string _initial = "?";
    public string Initial
    {
        get => _initial;
        set => SetProperty(ref _initial, value);
    }

    string _postsCount = "0";
    public string PostsCount
    {
        get => _postsCount;
        set => SetProperty(ref _postsCount, value);
    }

    string _followersCount = "0";
    public string FollowersCount
    {
        get => _followersCount;
        set => SetProperty(ref _followersCount, value);
    }

    string _followingCount = "0";
    public string FollowingCount
    {
        get => _followingCount;
        set => SetProperty(ref _followingCount, value);
    }

    // el botón cambia de aspecto solo (ver estilo FollowButton)
    bool _isFollowing;
    public bool IsFollowing
    {
        get => _isFollowing;
        set
        {
            if (SetProperty(ref _isFollowing, value))
            {
                RaisePropertyChanged(nameof(FollowText));
            }
        }
    }

    public string FollowText => IsFollowing ? "Siguiendo" : "Seguir";

    // el botón seguir no sale en tu propio perfil
    bool _isOtherUser;
    public bool IsOtherUser
    {
        get => _isOtherUser;
        set => SetProperty(ref _isOtherUser, value);
    }

    bool _hasNoPosts;
    public bool HasNoPosts
    {
        get => _hasNoPosts;
        set => SetProperty(ref _hasNoPosts, value);
    }

    // publicaciones del usuario
    ObservableCollection<Post> _posts = new();
    public ObservableCollection<Post> Posts
    {
        get => _posts;
        set => SetProperty(ref _posts, value);
    }

    // carga todos los datos del usuario
    public void Load(int id)
    {
        var user = db.Users.FirstOrDefault(u => u.Id == id);

        if (user is null)
        {
            return;
        }

        userId = id;

        Name = user.Name;
        Handle = "@" + user.Username;
        Initial = user.Username.ToInitial();

        var posts = db.Posts
            .Where(p => p.UserId == id)
            .OrderByDescending(p => p.Date)
            .ToList();

        Posts = new ObservableCollection<Post>(posts);
        HasNoPosts = posts.Count == 0;

        PostsCount = posts.Count.ToString();
        FollowersCount = follows.CountFollowers(id).ToString();
        FollowingCount = follows.CountFollowing(id).ToString();

        var current = Session.CurrentUser;
        IsOtherUser = current is not null && current.Id != id;
        IsFollowing = current is not null && follows.IsFollowing(current.Id, id);
    }

    // seguir o dejar de seguir
    public void ToggleFollow()
    {
        if (Session.CurrentUser is not { } current)
        {
            return;
        }

        follows.Toggle(current.Id, userId);

        // recargar para actualizar los números y el botón
        Load(userId);
    }
}