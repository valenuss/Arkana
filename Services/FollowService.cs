using Arkana.Data;
using Arkana.Models;

namespace Arkana.Services;

public class FollowService
{
    readonly AppDbContext db;

    public FollowService(AppDbContext db)
    {
        this.db = db;
    }

    public HashSet<int> GetFollowingIds(int userId) =>
        db.Follows
            .Where(f => f.FollowerId == userId)
            .Select(f => f.FollowingId)
            .ToHashSet();

    public bool IsFollowing(int followerId, int followingId) =>
        db.Follows.Any(f => f.FollowerId == followerId && f.FollowingId == followingId);

    public int CountFollowers(int userId) => db.Follows.Count(f => f.FollowingId == userId);

    public int CountFollowing(int userId) => db.Follows.Count(f => f.FollowerId == userId);

    // si ya lo sigue, deja de seguirlo; si no, lo empieza a seguir
    public void Toggle(int followerId, int followingId)
    {
        var existing = db.Follows.FirstOrDefault(f =>
            f.FollowerId == followerId && f.FollowingId == followingId);

        if (existing is null)
        {
            db.Follows.Add(new Follow { FollowerId = followerId, FollowingId = followingId });
        }
        else
        {
            db.Follows.Remove(existing);
        }

        db.SaveChanges();
    }
}