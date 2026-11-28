using Arkana.Data;
using Arkana.Pages;
using Arkana.Services;
using Microsoft.Extensions.Logging;

namespace Arkana;

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
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
            });

        // base de datos y servicios: una sola instancia para toda la app
        builder.Services.AddSingleton<AppDbContext>();
        builder.Services.AddSingleton<FollowService>();
        builder.Services.AddSingleton<NotificationService>();
        builder.Services.AddSingleton<StoryService>();
        builder.Services.AddSingleton<ChatService>();
        builder.Services.AddTransient<ChatPage>();

        // páginas (reciben sus dependencias en el constructor)
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<SearchPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<CreatePostPage>();
        builder.Services.AddTransient<UserProfilePage>();
        builder.Services.AddTransient<CommentsPage>();
        builder.Services.AddTransient<NotificationsPage>();
        builder.Services.AddTransient<StoryViewerPage>();
        builder.Services.AddSingleton<ChatService>();
        builder.Services.AddTransient<ChatPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}