using Arkana.Data;
using Arkana.Models;
using Arkana.Services;

namespace Arkana.Pages;

public partial class LoginPage : ContentPage
{
    // la base de datos llega desde MauiProgram
    readonly AppDbContext db;

    public LoginPage(AppDbContext db)
    {
        InitializeComponent();
        this.db = db;
    }

    // cuando le dan al boton de iniciar sesion
    private async void LoginButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            string email = EmailEntry.Text;
            string password = PasswordEntry.Text;

            // revisar que no dejen nada vacio
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Debes llenar todos los campos.", "Aceptar");
                return;
            }

            // buscar el usuario por correo o por nombre de usuario
            User usuario = db.Users.FirstOrDefault(u =>
                (u.Email == email || u.Username == email) && u.Password == password);

            if (usuario == null)
            {
                await DisplayAlert(
                    "Error",
                    "El correo o la contraseña son incorrectos.",
                    "Aceptar");
                return;
            }

            // guardar quien entro y limpiar los campos
            Session.CurrentUser = usuario;
            EmailEntry.Text = "";
            PasswordEntry.Text = "";

            // ir a la barra de pestañas
            await Shell.Current.GoToAsync("//main");
        }
        catch (Exception ex)
        {
            // muestra el error real si algo falla
            await DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }

    // mostrar u ocultar la contraseña
    private void ShowPassword_Tapped(object sender, TappedEventArgs e)
    {
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;
    }

    // ir a la pantalla de registro (por su ruta)
    private async void Register_Tapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("register");
    }
}