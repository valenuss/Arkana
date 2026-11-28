using Arkana.Helpers;
using Arkana.Pages;
using Arkana.Services;

namespace Arkana;

public partial class AppShell : Shell
{
    const string DocsUrl = "https://learn.microsoft.com/es-es/dotnet/maui/";

    public AppShell()
    {
        InitializeComponent();

        // rutas para abrir páginas por nombre
        Routing.RegisterRoute(Routes.Register, typeof(RegisterPage));
        Routing.RegisterRoute(Routes.CreatePost, typeof(CreatePostPage));
        Routing.RegisterRoute(Routes.UserProfile, typeof(UserProfilePage));
        Routing.RegisterRoute(Routes.Comments, typeof(CommentsPage));
        Routing.RegisterRoute(Routes.Notifications, typeof(NotificationsPage));
        Routing.RegisterRoute(Routes.StoryViewer, typeof(StoryViewerPage));
        Routing.RegisterRoute(Routes.Chat, typeof(ChatPage));
    }

    private async void About_Clicked(object? sender, EventArgs e)
    {
        FlyoutIsPresented = false;
        await DisplayAlert("Acerca de", $"{AppInfo.Name} {AppInfo.VersionString}", "Aceptar");
    }

    private async void Docs_Clicked(object? sender, EventArgs e)
    {
        FlyoutIsPresented = false;
        await Browser.Default.OpenAsync(new Uri(DocsUrl), BrowserLaunchMode.SystemPreferred);
    }

    private async void Logout_Clicked(object? sender, EventArgs e)
    {
        FlyoutIsPresented = false;
        await AuthNavigation.SignOutAsync();
    }
}