using Arkana.Data;
using Arkana.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace Arkana
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });


        // base de datos: una sola instancia accesible desde toda la app
        builder.Services.AddSingleton<AppDbContext>();

        // paginas (reciben el AppDbContext en su constructor)
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<SearchPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<CreatePostPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();

        }   
    }
}