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
            .ToList()
            .Select(p => new PostItem
            {
                Id = p.Id,
                Username = p.Username,
                Initial = p.Username.ToInitial(),
                TimeAgo = p.Date.ToTimeAgo(),
                Content = p.Content,
                Photo = string.IsNullOrEmpty(p.ImagePath) ? null : ImageSource.FromFile(p.ImagePath),
                CommentsCount = db.Comments.Count(c => c.PostId == p.Id),
                Likes = p.Likes,
                Liked = LikeTracker.IsLiked(p.Id)
            })
            .ToList();
    }

    // dar y quitar me gusta (con animación de latido)
    private async void Like_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Label { BindingContext: PostItem item } heart)
        {
            return;
        }

        item.Liked = !item.Liked;
        item.Likes += item.Liked ? 1 : -1;
        LikeTracker.Set(item.Id, item.Liked);

        // guardar el nuevo número en la base de datos
        var post = db.Posts.Find(item.Id);

        if (post is not null)
        {
            post.Likes = item.Likes;
            db.SaveChanges();
        }

        // latido: crece y vuelve a su tamaño
        await heart.ScaleTo(1.4, 100, Easing.CubicOut);
        await heart.ScaleTo(1.0, 100, Easing.CubicIn);
    }

    // abrir la pantalla de publicar
    private async void NewPost_Tapped(object? sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync(Routes.CreatePost);

    // la lupa del encabezado abre la pestaña Buscar
    private async void Search_Tapped(object? sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("//main/search");

    // cerrar sesión y volver al login
    private async void Logout_Tapped(object? sender, TappedEventArgs e) =>
        await AuthNavigation.SignOutAsync();

    // abrir los comentarios de una publicación
    private async void Comments_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is HorizontalStackLayout { BindingContext: PostItem item })
        {
            await Shell.Current.GoToAsync($"{Routes.Comments}?postId={item.Id}");
        }
    }

    // la campanita abre las notificaciones
    private async void Bell_Tapped(object? sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync(Routes.Notifications);

    // compartir una publicación con el menú del sistema
    private async void Share_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Label { BindingContext: PostItem item })
        {
            return;
        }

        await Share.Default.RequestAsync(new ShareTextRequest
        {
            Title = "Compartir publicación",
            Text = $"@{item.Username} en Arkana: {item.Content}"
        });
    } 
}