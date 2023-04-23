using F1.Domain.Model;

namespace F1.Domain.Repository;

public class MessageRepository : IMessageRepository
{
    private static IMessageRepository _instance;
    private List<Message> _messages;
    
    private MessageRepository()
    {
        _messages = new List<Message>();
    }
    public static IMessageRepository Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new MessageRepository();
            }
            return _instance;
        }
    }
    
    public void AddMessage(Message message)
    {
        _messages.Add(message);
    }

    public List<Message> GetUnreadMessagesForUser(string receiverId)
    {
        return _messages.FindAll(m => m.ReceiverId == receiverId && m.Seen == false);
    }

    public List<Message> GetAllMessagesForUser(string receiverId)
    {
        var userMessages = _messages.FindAll(m => m.ReceiverId == receiverId);
        userMessages.ForEach(m => m.Seen = true);

        return userMessages;
    }
}