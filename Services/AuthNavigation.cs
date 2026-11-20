using Arkana.Helpers;

namespace Arkana.Services;

public static class AuthNavigation
{
    // cierra la sesión y vuelve al login
    public static Task SignOutAsync()
    {
        Session.CurrentUser = null;
        LikeTracker.Clear();
        return Shell.Current.GoToAsync($"//{Routes.Login}");
    }
}