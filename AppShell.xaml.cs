using Arkana.Pages;
using Arkana.Services;

namespace Arkana;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // rutas para abrir paginas por nombre
        Routing.RegisterRoute("register", typeof(RegisterPage));
        Routing.RegisterRoute("createpost", typeof(CreatePostPage));
        Routing.RegisterRoute(nameof(UserProfilePage), typeof(UserProfilePage));
        Routing.RegisterRoute(nameof(CommentsPage), typeof(CommentsPage));
    }

    private async void About_Clicked(object sender, EventArgs e)
    {
        FlyoutIsPresented = false;
        await DisplayAlert("Acerca de", $"{AppInfo.Name} {AppInfo.VersionString}", "Aceptar");
    }

    private async void Docs_Clicked(object sender, EventArgs e)
    {
        FlyoutIsPresented = false;
        var uri = new Uri("https://learn.microsoft.com/es-es/dotnet/maui/");
        await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
    }

    private async void Logout_Clicked(object sender, EventArgs e)
    {
        Session.CurrentUser = null;
        FlyoutIsPresented = false;
        await GoToAsync("//login");
    }
}