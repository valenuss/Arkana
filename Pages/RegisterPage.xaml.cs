using Arkana.Data;
using Arkana.Models;

namespace Arkana.Pages;

public partial class RegisterPage : ContentPage
{
    readonly AppDbContext db;

    public RegisterPage(AppDbContext db)
    {
        InitializeComponent();
        this.db = db;
    }

    // botón de crear cuenta
    private async void RegisterButton_Clicked(object? sender, EventArgs e)
    {
        var name = NameEntry.Text?.Trim();
        var username = UsernameEntry.Text?.Trim();
        var email = EmailEntry.Text?.Trim();
        var password = PasswordEntry.Text;
        var confirmPassword = ConfirmPasswordEntry.Text;

        // que no falte ningún campo
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(confirmPassword))
        {
            await DisplayAlert("Error", "Debes llenar todos los campos.", "Aceptar");
            return;
        }

        // las contraseñas tienen que ser iguales
        if (password != confirmPassword)
        {
            await DisplayAlert("Error", "Las contraseñas no coinciden.", "Aceptar");
            return;
        }

        // que el correo o el usuario no existan ya
        if (db.Users.Any(u => u.Email == email || u.Username == username))
        {
            await DisplayAlert("Error", "El correo o el nombre de usuario ya existe.", "Aceptar");
            return;
        }

        db.Users.Add(new User
        {
            Name = name,
            Username = username,
            Email = email,
            Password = password
        });
        db.SaveChanges();

        await DisplayAlert("Arkana", "Cuenta creada correctamente.", "Aceptar");
        await GoBackAsync();
    }

    // tocar "Inicia sesión" para volver
    private async void Login_Tapped(object? sender, TappedEventArgs e) => await GoBackAsync();

    static Task GoBackAsync() => Shell.Current.GoToAsync("..");
}