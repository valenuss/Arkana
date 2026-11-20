namespace Arkana.Services;

public static class LikeTracker
{
    static readonly HashSet<int> likedPostIds = new();

    public static bool IsLiked(int postId) => likedPostIds.Contains(postId);

    public static void Set(int postId, bool liked)
    {
        if (liked)
        {
            likedPostIds.Add(postId);
        }
        else
        {
            likedPostIds.Remove(postId);
        }
    }

    public static void Clear() => likedPostIds.Clear();
}