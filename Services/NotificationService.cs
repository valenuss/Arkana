using Arkana.Data;
using Arkana.Helpers;
using Arkana.Models;

namespace Arkana.Services;

// datos de cada notificación tal como se muestra
public record NotificationItem(string Icon, string Message, string TimeAgo);


/// <summary>Arma las notificaciones y lleva la cuenta de las que ya se vieron.</summary>
public class NotificationService
{
    // cuántas notificaciones ya vio el usuario (se reinicia al cerrar sesión)
    static int seenCount;

    readonly AppDbContext db;

    public NotificationService(AppDbContext db)
    {
        this.db = db;
    }

    // comentarios en mis publicaciones y nuevos seguidores
    public List<NotificationItem> GetItems(int userId)
    {
        var myPostIds = db.Posts
            .Where(p => p.UserId == userId)
            .Select(p => p.Id)
            .ToList();

        var comments = db.Comments
            .Where(c => myPostIds.Contains(c.PostId) && c.UserId != userId)
            .ToList()
            .OrderByDescending(c => c.Date)
            .Select(c => new NotificationItem(
                Icons.ChatBubbleOutline,
                $"@{c.Username} comentó tu publicación: \"{c.Text}\"",
                c.Date.ToTimeAgo()));

        var followers = db.Follows
            .Where(f => f.FollowingId == userId)
            .ToList()
            .Select(f => db.Users.Find(f.FollowerId))
            .OfType<User>()
            .Select(u => new NotificationItem(
                Icons.PersonAdd,
                $"@{u.Username} empezó a seguirte",
                ""));

        return comments.Concat(followers).ToList();
    }

    // las que todavía no se han visto
    public int GetUnreadCount(int userId) =>
        Math.Max(0, GetItems(userId).Count - seenCount);

    // al abrir la pantalla de notificaciones
    public void MarkAllSeen(int userId) =>
        seenCount = GetItems(userId).Count;

    // al cerrar sesión
    public static void Reset() => seenCount = 0;
}