namespace Arkana.Models;

public class Story
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; } = "";
    public string ImagePath { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}