using Arkana.Data;
using Arkana.Helpers;
using Arkana.Models;
using Arkana.Services;

namespace Arkana.Pages;

public partial class HomePage : ContentPage
{
    readonly AppDbContext db;
    readonly NotificationService notifications;
    readonly FollowService follows;
    readonly StoryService stories;

    public HomePage(AppDbContext db, NotificationService notifications, FollowService follows, StoryService stories)
    {
        InitializeComponent();
        this.db = db;
        this.notifications = notifications;
        this.follows = follows;
        this.stories = stories;
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

        // contador de la campanita
        UpdateBellBadge(current.Id);

        // las historias de más de 24 horas se borran
        stories.PurgeExpired();

        LoadStories(current);
        LoadPosts();
    }

    // historias: solo de la gente que sigo y que tiene alguna vigente
    void LoadStories(User current)
    {
        var following = follows.GetFollowingIds(current.Id).ToList();

        var bubbles = db.Users
            .Where(u => following.Contains(u.Id))
            .ToList()
            .Where(u => stories.HasActive(u.Id))
            .Select(u => new StoryBubble(u.Id, u.Username, u.Username.ToInitial()))
            .ToList();

        BindableLayout.SetItemsSource(StoriesLayout, bubbles);
    }

    // publicaciones: las más nuevas primero
    void LoadPosts()
    {
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

    // muestra cuántas notificaciones nuevas hay
    void UpdateBellBadge(int userId)
    {
        var unread = notifications.GetUnreadCount(userId);

        BellBadge.IsVisible = unread > 0;
        BellBadgeLabel.Text = unread > 9 ? "9+" : unread.ToString();
    }

    // ---------- historias ----------

    // tocar mi foto: ver mi historia (o crearla si no tengo)
    private async void MyStory_Tapped(object? sender, TappedEventArgs e)
    {
        if (Session.CurrentUser is not { } me)
        {
            return;
        }

        if (!stories.HasActive(me.Id))
        {
            await AddStoryAsync(me);
            return;
        }

        var action = await DisplayActionSheet("Tu historia", "Cancelar", null, "Ver historia", "Agregar historia");

        if (action == "Ver historia")
        {
            await Shell.Current.GoToAsync($"{Routes.StoryViewer}?userId={me.Id}");
        }
        else if (action == "Agregar historia")
        {
            await AddStoryAsync(me);
        }
    }

    // tocar el +
    private async void AddStory_Tapped(object? sender, TappedEventArgs e)
    {
        if (Session.CurrentUser is { } me)
        {
            await AddStoryAsync(me);
        }
    }

    // elegir una foto y publicarla como historia (dura 24 horas)
    async Task AddStoryAsync(User me)
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Elige una foto para tu historia"
            });

            if (photo is null)
            {
                return;
            }

            await stories.AddAsync(me, photo);
            await DisplayAlert("Historia", "Tu historia se publicó y durará 24 horas.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    // tocar la historia de otra persona
    private async void Story_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is VerticalStackLayout { BindingContext: StoryBubble bubble })
        {
            await Shell.Current.GoToAsync($"{Routes.StoryViewer}?userId={bubble.UserId}");
        }
    }

    // ---------- publicaciones ----------

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

    // los 3 puntos: editar o eliminar (solo en publicaciones propias)
    private async void More_Tapped(object? sender, TappedEventArgs e)
    {
        if (sender is not Label { BindingContext: PostItem item })
        {
            return;
        }

        if (Session.CurrentUser is not { } me)
        {
            return;
        }

        var post = db.Posts.Find(item.Id);

        if (post is null)
        {
            return;
        }

        if (post.UserId != me.Id)
        {
            await DisplayAlert("Publicación", "Solo puedes editar o eliminar tus propias publicaciones.", "OK");
            return;
        }

        var action = await DisplayActionSheet("Publicación", "Cancelar", null, "Editar", "Eliminar");

        if (action == "Editar")
        {
            await EditPostAsync(post);
        }
        else if (action == "Eliminar")
        {
            await DeletePostAsync(post);
        }
    }

    // cambiar el texto de la publicación
    async Task EditPostAsync(Post post)
    {
        var newText = await DisplayPromptAsync(
            "Editar publicación",
            "Cambia el texto:",
            accept: "Guardar",
            cancel: "Cancelar",
            initialValue: post.Content,
            maxLength: 500,
            keyboard: Keyboard.Text);

        // null = canceló
        if (newText is null)
        {
            return;
        }

        newText = newText.Trim();

        // sin texto y sin foto no tiene sentido
        if (newText.Length == 0 && string.IsNullOrEmpty(post.ImagePath))
        {
            await DisplayAlert("Editar", "La publicación no puede quedar vacía.", "OK");
            return;
        }

        try
        {
            post.Content = newText;
            db.SaveChanges();
            LoadPosts();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    // borrar la publicación, sus comentarios y su foto
    async Task DeletePostAsync(Post post)
    {
        var confirm = await DisplayAlert(
            "Eliminar publicación",
            "¿Seguro que quieres eliminarla? Esta acción no se puede deshacer.",
            "Eliminar",
            "Cancelar");

        if (!confirm)
        {
            return;
        }

        try
        {
            // comentarios de esa publicación
            db.Comments.RemoveRange(db.Comments.Where(c => c.PostId == post.Id));

            // archivo de la foto (si tenía)
            if (!string.IsNullOrEmpty(post.ImagePath) && File.Exists(post.ImagePath))
            {
                File.Delete(post.ImagePath);
            }

            db.Posts.Remove(post);
            db.SaveChanges();

            LikeTracker.Set(post.Id, false);
            LoadPosts();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    // abrir la pantalla de publicar
    private async void NewPost_Tapped(object? sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync(Routes.CreatePost);

    // la lupa del encabezado abre la pestaña Buscar
    private async void Search_Tapped(object? sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("//main/search");

    // la campanita abre las notificaciones
    private async void Bell_Tapped(object? sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync(Routes.Notifications);

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


// datos de cada círculo de historia en el inicio
public record StoryBubble(int UserId, string Username, string Initial);