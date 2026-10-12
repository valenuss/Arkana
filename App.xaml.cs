using Arkana.Pages;
using Arkana.Services;

namespace Arkan

{
    public App()
    {
            InitializeComponent();
        }

    protected override Window CreateWindow(IActivationState? activationState)
        {
            FlyoutIsPresented = false;
            await GoToAsync(nameof(AboutPage));
        }
        private async void Docs_Clicked(object sender, EventArgs e)
        {
        {
            Session.CurrentUser = null;
            FlyoutIsPresented = false;
            await GoToAsync("//Login");
        }
    }
}