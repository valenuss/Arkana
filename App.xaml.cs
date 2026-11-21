namespace Arkana
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

#if WINDOWS
            // guarda en el escritorio el error que cierra la app
            Microsoft.UI.Xaml.Application.Current.UnhandledException += (s, e) =>
            {
                var path = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    "arkana-error.txt");
                File.WriteAllText(path, e.Exception?.ToString() ?? e.Message);
            };
#endif
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());

#if WINDOWS
            // tamaño de celular en Windows
            window.Width = 400;
            window.Height = 850;
            window.MinimumWidth = 360;
            window.MaximumWidth = 460;
            window.MinimumHeight = 640;
#endif

            return window;
        }
    }
}