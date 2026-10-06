using Arkana.Pages;
using Arkana.Services;

namespace Arkan

{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
        }

        private async void OnMenuItemClicked(object sender, EventArgs e)
        {
            FlyoutIsPresented = false;
            await GoToAsync(nameof(AboutPage));
        }
        private async void Docs_Clicked(object sender, EventArgs e)
        {
            FlyoutIsPresented = false;
            var uri = new Uri("https://learn.microsoft.com/es-es/dotnet/maui/")
                await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
        }

        private async void logout_Clicked(object sender, EventArgs e)
        {
            Session.CurrentUser = null;
            FlyoutIsPresented = false;
            await GoToAsync("//Login");
        }
    }
}