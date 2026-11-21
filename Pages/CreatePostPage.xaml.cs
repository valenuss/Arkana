using Arkana.Data;
using Arkana.Helpers;
using Arkana.Models;
using Arkana.Services;

namespace Arkana.Pages;

public partial class CreatePostPage : ContentPage
{
    const int MaxLength = 280;

    readonly AppDbContext db;

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
        PublishButton.IsEnabled = false;
    }

    // actualizar el contador y habilitar "Publicar" solo si hay texto
    private void ContentEditor_TextChanged(object? sender, TextChangedEventArgs e)
    {
        var length = e.NewTextValue?.Length ?? 0;
        CounterLabel.Text = $"{length} / {MaxLength}";
        PublishButton.IsEnabled = !string.IsNullOrWhiteSpace(e.NewTextValue);
    }

    // guardar la publicación
    private async void Publish_Clicked(object? sender, EventArgs e)
    {
        try
        {
            var text = ContentEditor.Text?.Trim();

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            if (Session.CurrentUser is not { } current)
            {
                return;
            }

            db.Posts.Add(new Post
            {
                UserId = current.Id,
                Username = current.Username,
                Content = text,
                Date = DateTime.Now,
                Likes = 0
            });
            db.SaveChanges();

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }

    // cerrar sin publicar
    private async void Close_Tapped(object? sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("..");
}