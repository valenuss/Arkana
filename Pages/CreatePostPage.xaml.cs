using Arkana.Data;
using Arkana.Helpers;
using Arkana.Models;
using Arkana.Services;

namespace Arkana.Pages;

public partial class CreatePostPage : ContentPage
{
    const int MaxLength = 280;

    readonly AppDbContext db;

    // ruta de la foto elegida (null si no hay)
    string? selectedImagePath;

    public CreatePostPage(AppDbContext db)
    {
        InitializeComponent();
        this.db = db;
    }

    // mostrar quién está publicando cada vez que se abre
    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (Session.CurrentUser is { } current)
        {
            UserAvatar.Initial = current.Username.ToInitial();
            NameLabel.Text = current.Name;
            UsernameLabel.Text = "@" + current.Username;
        }

        ContentEditor.Text = string.Empty;
        ClearPhoto();
        UpdatePublishState();
    }

    // se puede publicar si hay texto o una foto
    void UpdatePublishState() =>
        PublishButton.IsEnabled =
            !string.IsNullOrWhiteSpace(ContentEditor.Text) || selectedImagePath is not null;

    void ClearPhoto()
    {
        selectedImagePath = null;
        PreviewImage.Source = null;
        PreviewContainer.IsVisible = false;
    }

    // actualizar el contador
    private void ContentEditor_TextChanged(object? sender, TextChangedEventArgs e)
    {
        var length = e.NewTextValue?.Length ?? 0;
        CounterLabel.Text = $"{length} / {MaxLength}";
        UpdatePublishState();
    }

    // elegir una foto de la galería
    private async void PickPhoto_Tapped(object? sender, TappedEventArgs e)
    {
        try
        {
            var file = await MediaPicker.Default.PickPhotoAsync();

            if (file is null)
            {
                return;
            }

            // copiar la foto a la carpeta de la app para no perderla
            var path = Path.Combine(
                FileSystem.AppDataDirectory,
                $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}");

            using (var source = await file.OpenReadAsync())
            using (var target = File.Create(path))
            {
                await source.CopyToAsync(target);
            }

            selectedImagePath = path;
            PreviewImage.Source = ImageSource.FromFile(path);
            PreviewContainer.IsVisible = true;
            UpdatePublishState();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo cargar la foto: {ex.Message}", "Aceptar");
        }
    }

    // quitar la foto elegida
    private void RemovePhoto_Tapped(object? sender, TappedEventArgs e)
    {
        ClearPhoto();
        UpdatePublishState();
    }

    // guardar la publicación
    private async void Publish_Clicked(object? sender, EventArgs e)
    {
        try
        {
            var text = ContentEditor.Text?.Trim() ?? string.Empty;

            if (text.Length == 0 && selectedImagePath is null)
            {
                await DisplayAlert("Error", "Escribe algo o agrega una foto.", "Aceptar");
                return;
            }

            if (Session.CurrentUser is not { } current)
            {
                return;
            }

            // el siguiente id libre (las publicaciones de ejemplo ya usan del 1 al 5)
            var nextId = db.Posts.Any() ? db.Posts.Max(p => p.Id) + 1 : 1;

            db.Posts.Add(new Post
            {
                Id = nextId,
                UserId = current.Id,
                Username = current.Username,
                Content = text,
                ImagePath = selectedImagePath,
                Date = DateTime.Now,
                Likes = 0
            });
            db.SaveChanges();

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.ToString(), "Aceptar");
        }
    }

    // cerrar sin publicar
    private async void Close_Tapped(object? sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("..");
}