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

    // boton de crear cuenta
    private async void RegisterButton_Clicked(object sender, EventArgs e)
    {
        // sacar lo que escribieron (sin espacios de mas al inicio o al final)
        string name = (NameEntry.Text ?? "").Trim();
        string username = (UsernameEntry.Text ?? "").Trim();
        string email = (EmailEntry.Text ?? "").Trim();
        string password = PasswordEntry.Text ?? "";
        string confirmPassword = ConfirmPasswordEntry.Text ?? "";

        // que no falte ningun campo
        if (name == "" || username == "" || email == "" ||
            string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
        {
            await DisplayAlert("Error", "Debes llenar todos los campos.", "Aceptar");
            return;
        }

        // el nombre de usuario: minimo 3 letras y sin espacios
        if (username.Length < 3 || username.Contains(" "))
        {
            await DisplayAlert("Error", "El usuario debe tener al menos 3 caracteres y no puede llevar espacios.", "Aceptar");
            return;
        }

        // el correo tiene que verse como un correo
        bool correoValido = email.Contains("@") && email.Contains(".") && !email.Contains(" ");

        if (correoValido == false)
        {
            await DisplayAlert("Error", "Escribe un correo válido, por ejemplo nombre@correo.com.", "Aceptar");
            return;
        }

        // la contraseña tiene que tener un minimo
        if (password.Length < 6)
        {
            await DisplayAlert("Error", "La contraseña debe tener al menos 6 caracteres.", "Aceptar");
            return;
        }

        // las contraseñas tienen que ser iguales
        if (password != confirmPassword)
        {
            await DisplayAlert("Error", "Las contraseñas no coinciden.", "Aceptar");
            return;
        }

        // revisar que el correo o el usuario no existan
        bool existe = db.Users.Any(u => u.Email == email || u.Username == username);

        if (existe)
        {
            await DisplayAlert("Error", "El correo o el nombre de usuario ya existe.", "Aceptar");
            return;
        }

        // crear el usuario nuevo y guardarlo
        User usuario = new User();
        usuario.Name = name;
        usuario.Username = username;
        usuario.Email = email;
        usuario.Password = password;

        db.Users.Add(usuario);
        db.SaveChanges();

        await DisplayAlert("Arkana", "Cuenta creada correctamente.", "Aceptar");

        // volver al login
        await Navigation.PopAsync();
    }

    // tocar "Inicia sesion" para volver
    private async void Login_Tapped(object sender, TappedEventArgs e)
    {
        await Navigation.PopAsync();
    }
}