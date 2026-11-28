using Arkana.Data;
using Arkana.Models;

namespace Arkana.Services;

/// <summary>Historias con foto que duran 24 horas, con sus me gusta.</summary>
public class StoryService
{
    static readonly TimeSpan Lifetime = TimeSpan.FromHours(24);

    readonly AppDbContext db;

    public StoryService(AppDbContext db)
    {
        this.db = db;
    }

    // borra las historias que ya cumplieron 24 horas (fila, me gusta y foto)
    public void PurgeExpired()
    {
        var limit = DateTime.Now - Lifetime;

        var expired = db.Stories
            .Where(s => s.CreatedAt <= limit)
            .ToList();

        if (expired.Count == 0)
        {
            return;
        }

        var ids = expired.Select(s => s.Id).ToList();

        foreach (var story in expired)
        {
            DeleteFile(story.ImagePath);
        }

        db.StoryLikes.RemoveRange(db.StoryLikes.Where(l => ids.Contains(l.StoryId)));
        db.Stories.RemoveRange(expired);
        db.SaveChanges();
    }

    // historias vigentes de un usuario, de la más vieja a la más nueva
    public List<Story> GetActive(int userId)
    {
        var limit = DateTime.Now - Lifetime;

        return db.Stories
            .Where(s => s.UserId == userId && s.CreatedAt > limit)
            .ToList()
            .OrderBy(s => s.CreatedAt)
            .ToList();
    }

    public bool HasActive(int userId) => GetActive(userId).Count > 0;

    // guarda la foto en la carpeta de la app y crea la historia
    public async Task AddAsync(User user, FileResult photo)
    {
        var fileName = $"story_{Guid.NewGuid():N}{Path.GetExtension(photo.FileName)}";
        var path = Path.Combine(FileSystem.AppDataDirectory, fileName);

        using (var input = await photo.OpenReadAsync())
        using (var output = File.Create(path))
        {
            await input.CopyToAsync(output);
        }

        db.Stories.Add(new Story
        {
            UserId = user.Id,
            Username = user.Username,
            ImagePath = path,
            CreatedAt = DateTime.Now
        });

        db.SaveChanges();
    }

    // elimina una historia, sus me gusta y su foto
    public void Delete(Story story)
    {
        DeleteFile(story.ImagePath);
        db.StoryLikes.RemoveRange(db.StoryLikes.Where(l => l.StoryId == story.Id));
        db.Stories.Remove(story);
        db.SaveChanges();
    }

    // ---------- me gusta ----------

    public bool IsLikedBy(int storyId, int userId) =>
        db.StoryLikes.Any(l => l.StoryId == storyId && l.UserId == userId);

    public int CountLikes(int storyId) =>
        db.StoryLikes.Count(l => l.StoryId == storyId);

    // da o quita el me gusta; devuelve true si quedó con me gusta
    public bool ToggleLike(int storyId, int userId)
    {
        var like = db.StoryLikes.FirstOrDefault(l => l.StoryId == storyId && l.UserId == userId);

        if (like is null)
        {
            db.StoryLikes.Add(new StoryLike { StoryId = storyId, UserId = userId });
            db.SaveChanges();
            return true;
        }

        db.StoryLikes.Remove(like);
        db.SaveChanges();
        return false;
    }

    static void DeleteFile(string path)
    {
        try
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch
        {
            // si no se puede borrar la foto, no pasa nada
        }
    }
}