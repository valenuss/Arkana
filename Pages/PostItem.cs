using Arkana.Helpers;

namespace Arkana.Pages;

// datos de cada historia
public record StoryItem(string Username, string Initial);


// datos de cada publicación tal como se muestra en pantalla
public class PostItem : BindingUtilObject
{
    public int Id { get; init; }
    public string Username { get; init; } = "";
    public string Initial { get; init; } = "?";
    public string TimeAgo { get; init; } = "";
    public string Content { get; init; } = "";
    public int CommentsCount { get; init; }
    public ImageSource? Photo { get; init; }
    public bool HasImage => Photo is not null;

    int _likes;
    public int Likes
    {
        get => _likes;
        set => SetProperty(ref _likes, value);
    }

    bool _liked;
    public bool Liked
    {
        get => _liked;
        set
        {
            if (SetProperty(ref _liked, value))
            {
                RaisePropertyChanged(nameof(Heart));
            }
        }
    }

    // el corazón cambia según si ya le diste me gusta
    public string Heart => Liked ? Icons.Favorite : Icons.FavoriteBorder;
}