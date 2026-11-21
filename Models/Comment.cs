namespace Arkana.Models;

public class Comment
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = "";
    public string Text { get; set; } = "";
    public DateTime Date { get; set; }
}