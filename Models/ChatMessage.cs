namespace Arkana.Models;

public class ChatMessage
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public string Text { get; set; } = "";
    public DateTime Date { get; set; }
    public bool IsStoryReply { get; set; }
}