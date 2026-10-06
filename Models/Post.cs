namespace Arkana.Models;

public class Post
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Content { get; set; }
    public DateTime Date { get; set; }
    public int Likes { get; set; }
}