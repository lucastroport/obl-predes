using F1.Domain.Model;

namespace F1.Domain.Repository;

public interface IMessageRepository
{
    void AddMessage(Message message);
    List<Message> GetUnreadMessagesForUser(string receiverId);
    List<Message> GetAllMessagesForUser(string receiverId);
}