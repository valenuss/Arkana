using Arkana.Pages;

namespace Arkana
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("register", typeof(RegisterPage));
            Routing.RegisterRoute("createpost", typeof(CreatePostPage));
        }
    }
}