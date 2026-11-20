using Arkana.Data;
using Arkana.Helpers;
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

    // cuando le dan al botón de iniciar sesión
    private async void LoginButton_Clicked(object? sender, EventArgs e)
    {
        try
        {
            var email = EmailEntry.Text?.Trim();
            var password = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Debes llenar todos los campos.", "Aceptar");
                return;
            }

            // buscar el usuario por correo o por nombre de usuario
            var user = db.Users.FirstOrDefault(u =>
                (u.Email == email || u.Username == email) && u.Password == password);

            if (user is null)
            {
                await DisplayAlert("Error", "El correo o la contraseña son incorrectos.", "Aceptar");
                return;
            }

            // guardar quién entró y limpiar los campos
            Session.CurrentUser = user;
            EmailEntry.Text = string.Empty;
            PasswordEntry.Text = string.Empty;

            await Shell.Current.GoToAsync($"//{Routes.Main}");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }

    // mostrar u ocultar la contraseña
    private void ShowPassword_Tapped(object? sender, TappedEventArgs e) =>
        PasswordEntry.IsPassword = !PasswordEntry.IsPassword;

    // ir a la pantalla de registro
    private async void Register_Tapped(object? sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync(Routes.Register);
}