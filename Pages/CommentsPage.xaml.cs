using Arkana.Data;
using Arkana.Models;
using Arkana.Services;

namespace Arkana.Pages;

public partial class CommentsPage : ContentPage, IQueryAttributable
{
    readonly AppDbContext db;
    int postId;

    public CommentsPage(AppDbContext db)
    {
        InitializeComponent();
        this.db = db;
    }

    // recibe el parametro de la ruta: CommentsPage?postId=3
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        postId = Convert.ToInt32(query["postId"]);
        CargarComentarios();
    }

    // llena la publicacion de arriba y la lista de comentarios
    void CargarComentarios()
    {
        Post post = db.Posts.Find(postId);

        if (post != null)
        {
            PostAuthorLabel.Text = "@" + post.Username;
            PostContentLabel.Text = post.Content;
        }

        // los mas viejos primero, como una conversacion
        List<CommentItem> lista = new List<CommentItem>();

        foreach (Comment c in db.Comments.Where(c => c.PostId == postId).OrderBy(c => c.Date).ToList())
        {
            CommentItem item = new CommentItem();
            item.Username = c.Username;
            item.Initial = c.Username.Substring(0, 1).ToUpper();
            item.TimeAgo = HaceCuanto(c.Date);
            item.Content = c.Content;
            lista.Add(item);
        }

        CommentsList.ItemsSource = lista;
    }

    // convierte la fecha en "hace 2 h"
    static string HaceCuanto(DateTime fecha)
    {
        TimeSpan tiempo = DateTime.Now - fecha;

        if (tiempo.TotalMinutes < 1)
        {
            return "ahora";
        }
        if (tiempo.TotalHours < 1)
        {
            return "hace " + (int)tiempo.TotalMinutes + " min";
        }
        if (tiempo.TotalDays < 1)
        {
            return "hace " + (int)tiempo.TotalHours + " h";
        }
        return "hace " + (int)tiempo.TotalDays + " d";
    }

    // boton enviar (o la tecla enviar del teclado)
    private async void Send_Clicked(object sender, EventArgs e)
    {
        try
        {
            string texto = (CommentEntry.Text ?? "").Trim();
            User actual = Session.CurrentUser;

            // que no este vacio
            if (texto == "" || actual == null)
            {
                return;
            }

            // crear el comentario y guardarlo
            Comment nuevo = new Comment();
            nuevo.PostId = postId;
            nuevo.UserId = actual.Id;
            nuevo.Username = actual.Username;
            nuevo.Content = texto;
            nuevo.Date = DateTime.Now;

            db.Comments.Add(nuevo);
            db.SaveChanges();

            // limpiar el campo y mostrar la lista actualizada
            CommentEntry.Text = "";
            CargarComentarios();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "Aceptar");
        }
    }

    // volver a la pantalla anterior
    private async void Back_Tapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}


// datos de cada comentario tal como se muestra en pantalla
public class CommentItem
{
    public string Username { get; set; }
    public string Initial { get; set; }
    public string TimeAgo { get; set; }
    public string Content { get; set; }
}