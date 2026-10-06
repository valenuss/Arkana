using System.Collections.ObjectModel;
using Arkana.Data;
using Arkana.Models;
using Arkana.Services;

namespace Arkana.Pages;

public partial class UserProfilePage : ContentPage, IQueryAttributable
{
    readonly UserProfileData data;

    public UserProfilePage(AppDbContext db)
    {
        InitializeComponent();

        data = new UserProfileData(db);
        BindingContext = data;
    }

    // recibe el parametro de la ruta: UserProfilePage?id=3
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        int id = Convert.ToInt32(query["id"]);
        data.Load(id);
    }

    // volver a la pantalla anterior
    private async void Back_Tapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    // seguir o dejar de seguir
    private void Follow_Clicked(object sender, EventArgs e)
    {
        data.ToggleFollow();
    }
}


// clase de datos del perfil de otro usuario
public class UserProfileData : BindingUtilObject
{
    readonly AppDbContext db;
    int userId;

    public UserProfileData(AppDbContext db)
    {
        this.db = db;
    }

    string _name = "";
    public string Name
    {
        get { return _name; }
        set { SetProperty(ref _name, value); }
    }

    string _handle = "";
    public string Handle
    {
        get { return _handle; }
        set { SetProperty(ref _handle, value); }
    }

    string _initial = "";
    public string Initial
    {
        get { return _initial; }
        set { SetProperty(ref _initial, value); }
    }

    string _postsCount = "0";
    public string PostsCount
    {
        get { return _postsCount; }
        set { SetProperty(ref _postsCount, value); }
    }

    string _followersCount = "0";
    public string FollowersCount
    {
        get { return _followersCount; }
        set { SetProperty(ref _followersCount, value); }
    }

    string _followingCount = "0";
    public string FollowingCount
    {
        get { return _followingCount; }
        set { SetProperty(ref _followingCount, value); }
    }

    string _buttonText = "Seguir";
    public string ButtonText
    {
        get { return _buttonText; }
        set { SetProperty(ref _buttonText, value); }
    }

    Color _buttonColor = Color.FromArgb("#FF1F3D");
    public Color ButtonColor
    {
        get { return _buttonColor; }
        set { SetProperty(ref _buttonColor, value); }
    }

    Color _buttonTextColor = Colors.White;
    public Color ButtonTextColor
    {
        get { return _buttonTextColor; }
        set { SetProperty(ref _buttonTextColor, value); }
    }

    // el boton seguir no sale en tu propio perfil
    bool _isOtherUser;
    public bool IsOtherUser
    {
        get { return _isOtherUser; }
        set { SetProperty(ref _isOtherUser, value); }
    }

    bool _hasNoPosts;
    public bool HasNoPosts
    {
        get { return _hasNoPosts; }
        set { SetProperty(ref _hasNoPosts, value); }
    }

    // publicaciones del usuario
    ObservableCollection<Post> _posts = new ObservableCollection<Post>();
    public ObservableCollection<Post> Posts
    {
        get { return _posts; }
        set { SetProperty(ref _posts, value); }
    }

    // carga todos los datos del usuario
    public void Load(int id)
    {
        userId = id;

        User? u = db.Users.FirstOrDefault(x => x.Id == id);

        if (u == null)
        {
            return;
        }

        Name = u.Name;
        Handle = "@" + u.Username;
        Initial = u.Username.Substring(0, 1).ToUpper();

        PostsCount = db.Posts.Count(p => p.UserId == id).ToString();
        FollowersCount = db.Follows.Count(f => f.FollowingId == id).ToString();
        FollowingCount = db.Follows.Count(f => f.FollowerId == id).ToString();

        List<Post> publicaciones = db.Posts
            .Where(p => p.UserId == id)
            .OrderByDescending(p => p.Date)
            .ToList();

        Posts = new ObservableCollection<Post>(publicaciones);
        HasNoPosts = publicaciones.Count == 0;

        User? actual = Session.CurrentUser;
        IsOtherUser = actual != null && actual.Id != id;

        ActualizarBoton();
    }

    // cambia el boton segun si ya lo sigo o no
    void ActualizarBoton()
    {
        User? actual = Session.CurrentUser;

        bool losSigo = actual != null &&
            db.Follows.Any(f => f.FollowerId == actual.Id && f.FollowingId == userId);

        if (losSigo)
        {
            ButtonText = "Siguiendo";
            ButtonColor = Color.FromArgb("#14141A");
            ButtonTextColor = Color.FromArgb("#9A9AA5");
        }
        else
        {
            ButtonText = "Seguir";
            ButtonColor = Color.FromArgb("#FF1F3D");
            ButtonTextColor = Colors.White;
        }
    }

    // seguir o dejar de seguir
    public void ToggleFollow()
    {
        User? actual = Session.CurrentUser;

        if (actual == null)
        {
            return;
        }

        Follow? existente = db.Follows.FirstOrDefault(f =>
            f.FollowerId == actual.Id && f.FollowingId == userId);

        if (existente != null)
        {
            db.Follows.Remove(existente);
        }
        else
        {
            Follow nuevo = new Follow();
            nuevo.FollowerId = actual.Id;
            nuevo.FollowingId = userId;
            db.Follows.Add(nuevo);
        }

        db.SaveChanges();

        // recargar para actualizar los numeros y el boton
        Load(userId);
    }
}