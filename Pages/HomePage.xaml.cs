using System.ComponentModel;
using Arkana.Data;
using Arkana.Models;
using Arkana.Services;

namespace Arkana.Pages;

public partial class HomePage : ContentPage
{
    readonly AppDbContext db;

    // ids de las publicaciones a las que ya le di me gusta
    static List<int> likedIds = new List<int>();

    public HomePage(AppDbContext db)
    {
        InitializeComponent();
        this.db = db;
    }

    // cada vez que se muestra el home, se cargan los datos
    protected override void OnAppearing()
    {
        base.OnAppearing();

        User actual = Session.CurrentUser;

        if (actual == null)
        {
            return;
        }

        // la inicial del usuario va en su circulo
        string inicial = actual.Username.Substring(0, 1).ToUpper();
        StoryInitialLabel.Text = inicial;
        ComposerInitialLabel.Text = inicial;

        // historias: los demas usuarios
        List<StoryItem> historias = new List<StoryItem>();

        foreach (User u in db.Users.Where(u => u.Id != actual.Id).ToList())
        {
            StoryItem historia = new StoryItem();
            historia.Username = u.Username;
            historia.Initial = u.Username.Substring(0, 1).ToUpper();
            historias.Add(historia);
        }

        BindableLayout.SetItemsSource(StoriesLayout, historias);

        // publicaciones: las mas nuevas primero
        List<PostItem> lista = new List<PostItem>();

        foreach (Post p in db.Posts.OrderByDescending(p => p.Date).ToList())
        {
            PostItem item = new PostItem();
            item.Id = p.Id;
            item.Username = p.Username;
            item.Initial = p.Username.Substring(0, 1).ToUpper();
            item.TimeAgo = HaceCuanto(p.Date);
            item.Content = p.Content;
            item.Likes = p.Likes;
            item.Liked = likedIds.Contains(p.Id);
            lista.Add(item);
        }

        PostsList.ItemsSource = lista;
    }

    // convierte la fecha en "hace 2 h"
    static string HaceCuanto(DateTime fecha)
    {
        TimeSpan tiempo = DateTime.Now - fecha;

        if (tiempo.TotalMinutes < 1)
        {
            return "ahora";
        }
        if (tiempo.TotalHours < 1)
        {
            return "hace " + (int)tiempo.TotalMinutes + " min";
        }
        if (tiempo.TotalDays < 1)
        {
            return "hace " + (int)tiempo.TotalHours + " h";
        }
        return "hace " + (int)tiempo.TotalDays + " d";
    }

    // dar y quitar me gusta
    private void Like_Tapped(object sender, TappedEventArgs e)
    {
        Label corazon = (Label)sender;
        PostItem item = (PostItem)corazon.BindingContext;

        if (item.Liked)
        {
            item.Liked = false;
            item.Likes--;
            likedIds.Remove(item.Id);
        }
        else
        {
            item.Liked = true;
            item.Likes++;
            likedIds.Add(item.Id);
        }

        // guardar el nuevo numero en la base de datos
        Post post = db.Posts.Find(item.Id);

        if (post != null)
        {
            post.Likes = item.Likes;
            db.SaveChanges();
        }
    }

    // abrir la pantalla de publicar (por su ruta)
    private async void NewPost_Tapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("createpost");
    }

    // cerrar sesion y volver al login
    private async void Logout_Clicked(object sender, TappedEventArgs e)
    {
        Session.CurrentUser = null;
        likedIds.Clear();
        await Shell.Current.GoToAsync("//login");
    }
}


// datos de cada historia
public class StoryItem
{
    public string Username { get; set; }
    public string Initial { get; set; }
}


// datos de cada publicacion tal como se muestra en pantalla
public class PostItem : INotifyPropertyChanged
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Initial { get; set; }
    public string TimeAgo { get; set; }
    public string Content { get; set; }

    int likes;
    public int Likes
    {
        get { return likes; }
        set
        {
            likes = value;
            OnChanged(nameof(Likes));
        }
    }

    bool liked;
    public bool Liked
    {
        get { return liked; }
        set
        {
            liked = value;
            OnChanged(nameof(Heart));
        }
    }

    // el corazon cambia segun si ya le diste me gusta
    public string Heart
    {
        get
        {
            if (Liked)
            {
                return "❤️";
            }
            return "🤍";
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    void OnChanged(string nombre)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombre));
    }
}