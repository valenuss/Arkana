using System.Collections.ObjectModel;
using Arkana.Data;
using Arkana.Helpers;
using Arkana.Models;
using Arkana.Services;

namespace Arkana.Pages;

public partial class CommentsPage : ContentPage, IQueryAttributable
{
    readonly CommentsData data;

    public CommentsPage(AppDbContext db)
    {
        InitializeComponent();

        data = new CommentsData(db);
        BindingContext = data;
    }

    // recibe el parámetro de la ruta: comments?postId=3
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("postId", out var id))
        {
            data.Load(Convert.ToInt32(id));
        }
    }

    private async void Back_Tapped(object? sender, TappedEventArgs e) =>
        await Shell.Current.GoToAsync("..");

    // enviar el comentario (botón o tecla Enter)
    private async void Send_Requested(object? sender, EventArgs e)
    {
        try
        {
            if (data.Send() && data.Comments.Count > 0)
            {
                CommentsList.ScrollTo(data.Comments.Count - 1, position: ScrollToPosition.End, animate: true);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.ToString(), "Aceptar");
        }
    }
}


// datos de la pantalla de comentarios
public class CommentsData : BindingUtilObject
{
    readonly AppDbContext db;
    int postId;

    public CommentsData(AppDbContext db)
    {
        this.db = db;
    }

    ObservableCollection<CommentItem> _comments = new();
    public ObservableCollection<CommentItem> Comments
    {
        get => _comments;
        set => SetProperty(ref _comments, value);
    }

    // lo que se está escribiendo
    string _newText = "";
    public string NewText
    {
        get => _newText;
        set => SetProperty(ref _newText, value);
    }

    public void Load(int id)
    {
        postId = id;
        Refresh();
    }

    void Refresh()
    {
        var items = db.Comments
            .Where(c => c.PostId == postId)
            .OrderBy(c => c.Date)
            .ToList()
            .Select(c => new CommentItem(c.Username, c.Username.ToInitial(), c.Text, c.Date.ToTimeAgo()))
            .ToList();

        Comments = new ObservableCollection<CommentItem>(items);
    }

    // guarda el comentario; devuelve true si se envió
    public bool Send()
    {
        if (Session.CurrentUser is not { } current || string.IsNullOrWhiteSpace(NewText))
        {
            return false;
        }

        // el siguiente id libre (los comentarios de ejemplo ya usan el 1 y el 2)
        var nextId = db.Comments.Any() ? db.Comments.Max(c => c.Id) + 1 : 1;

        db.Comments.Add(new Comment
        {
            Id = nextId,
            PostId = postId,
            UserId = current.Id,
            Username = current.Username,
            Text = NewText.Trim(),
            Date = DateTime.Now
        });
        db.SaveChanges();

        NewText = "";
        Refresh();
        return true;
    }
}


// datos de cada comentario tal como se muestra
public record CommentItem(string Username, string Initial, string Text, string TimeAgo);