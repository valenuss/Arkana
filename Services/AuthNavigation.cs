using Arkana.Helpers;

namespace Arkana.Services;

public static class AuthNavigation
{
    // cierra la sesión (en un solo lugar) y vuelve al login
    public static Task SignOutAsync()
    {
        Session.CurrentUser = null;
        LikeTracker.Clear();
        NotificationService.Reset();
        return Shell.Current.GoToAsync($"//{Routes.Login}");
    }
}