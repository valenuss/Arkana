using Arkana.Data;
using Arkana.Models;

namespace Arkana.Services;

/// <summary>Mensajes privados entre dos personas.</summary>
public class ChatService
{
    readonly AppDbContext db;

    public ChatService(AppDbContext db)
    {
        this.db = db;
    }

    // todos los mensajes entre dos usuarios, en orden
    public List<ChatMessage> GetConversation(int userA, int userB) =>
        db.ChatMessages
            .Where(m => (m.SenderId == userA && m.ReceiverId == userB)
                     || (m.SenderId == userB && m.ReceiverId == userA))
            .ToList()
            .OrderBy(m => m.Date)
            .ToList();

    public void Send(int senderId, int receiverId, string text, bool isStoryReply = false)
    {
        db.ChatMessages.Add(new ChatMessage
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            Text = text,
            Date = DateTime.Now,
            IsStoryReply = isStoryReply
        });

        db.SaveChanges();
    }
}