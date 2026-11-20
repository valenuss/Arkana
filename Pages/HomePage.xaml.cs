using Arkana.Data;
using Arkana.Helpers;
using Arkana.Services;

namespace Arkana.Pages;

public partial class HomePage : ContentPage
{
    readonly AppDbContext db;

    public HomePage(AppDbContext db)
    {
        InitializeComponent();
        this.db = db;
    }

    // cada vez que se muestra el inicio, se cargan los datos
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (Session.CurrentUser is not { } current)
        {
            return;
        }

        // la inicial del usuario va en su círculo
        var initial = current.Username.ToInitial();
        StoryAvatar.Initial = initial;
        ComposerAvatar.Initial = initial;

        // historias: los demás usuarios
        var stories = db.Users
            .Where(u => u.Id != current.Id)
            .AsEnumerable()
            .Select(u => new StoryItem(u.Username, u.Username.ToInitial()))
            .ToList();

        BindableLayout.SetItemsSource(StoriesLayout, stories);

        // publicaciones: las más nuevas primero
        PostsList.ItemsSource = db.Posts
            .OrderByDescending(p => p.Date)
            .AsEnumerable()
            .Select(p => new PostItem
            {
                Id = p.Id,
                Username = p.Username,
                Initial = p.Username.ToInitial(),
                TimeAgo = p.Date.ToTimeAgo(),
                Content = p.Content,
                Likes = p.Likes,
                Liked = LikeTracker.IsLiked(p.Id)
            })
            .ToList();
    }

    // dar y quitar me gusta
    private void Like_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Label { BindingContext: PostItem item })
        {
            return;
        }

        item.Liked = !item.Liked;
        item.Likes += item.Liked ? 1 : -1;
        LikeTracker.Set(item.Id, item.Liked);

        // guardar el nuevo número en la base de datos
        var post = db.Posts.Find(item.Id);

        if (post is null)
        {
            return;
        }

        post.Likes = item.Likes;
        db.SaveChanges();
    }

    // abrir la pantalla de publicar
    private async void NewPost_Tapped(object? sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync(Routes.CreatePost);

    // cerrar sesión y volver al login
    private async void Logout_Tapped(object? sender, TappedEventArgs e) =>
        await AuthNavigation.SignOutAsync();
}


// datos de cada historia
public record StoryItem(string Username, string Initial);


// datos de cada publicación tal como se muestra en pantalla
public class PostItem : BindingUtilObject
{
    public int Id { get; init; }
    public string Username { get; init; } = "";
    public string Initial { get; init; } = "?";
    public string TimeAgo { get; init; } = "";
    public string Content { get; init; } = "";

    int _likes;
    public int Likes
    {
        get => _likes;
        set => SetProperty(ref _likes, value);
    }

    bool _liked;
    public bool Liked
    {
        get => _liked;
        set
        {
            if (SetProperty(ref _liked, value))
            {
                RaisePropertyChanged(nameof(Heart));
            }
        }
    }

    // el corazón cambia según si ya le diste me gusta
    public string Heart => Liked ? "❤️" : "🤍";
}