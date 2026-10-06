using Arkana.Data;
using Arkana.Models;
using Arkana.Services;

namespace Arkana.Pages;

public partial class CreatePostPage : ContentPage
{
    readonly AppDbContext db;

    public CreatePostPage(AppDbContext db)
    {
        InitializeComponent();
        this.db = db;
    }

    // mostrar quien esta publicando cada vez que se abre
    protected override void OnAppearing()
    {
        base.OnAppearing();

        User actual = Session.CurrentUser;

        if (actual != null)
        {
            InitialLabel.Text = actual.Username.Substring(0, 1).ToUpper();
            NameLabel.Text = actual.Name;
            UsernameLabel.Text = "@" + actual.Username;
        }

        ContentEditor.Text = "";
    }

    // actualizar el contador de letras
    private void ContentEditor_TextChanged(object sender, TextChangedEventArgs e)
    {
        int cantidad = 0;

        if (e.NewTextValue != null)
        {
            cantidad = e.NewTextValue.Length;
        }

        CounterLabel.Text = cantidad + " / 280";
    }

    // guardar la publicacion
    private async void Publish_Clicked(object sender, EventArgs e)
    {
        try
        {
            string texto = ContentEditor.Text;

            // que no este vacia
            if (string.IsNullOrWhiteSpace(texto))
            {
                await DisplayAlert("Error", "Escribe algo antes de publicar.", "Aceptar");
                return;
            }

            User actual = Session.CurrentUser;

            // crear la publicacion
            Post nuevo = new Post();
            nuevo.UserId = actual.Id;
            nuevo.Username = actual.Username;
            nuevo.Content = texto.Trim();
            nuevo.Date = DateTime.Now;
            nuevo.Likes = 0;

            // guardarla en la base de datos
            db.Posts.Add(nuevo);
            db.SaveChanges();

            // volver al home
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }

    // cerrar sin publicar
    private async void Close_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}