using Arkana.Pages;

namespace Arkana
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // rutas para abrir paginas por nombre
            Routing.RegisterRoute("register", typeof(RegisterPage));
            Routing.RegisterRoute("createpost", typeof(CreatePostPage));
        }
    }
}