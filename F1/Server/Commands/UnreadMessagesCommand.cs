using F1.Domain.Comparator;
using F1.Domain.Model;
using F1.Domain.Repository;
using F1.Presentation.Views.Menu;

namespace Server.Commands;

public class UnreadMessagesCommand : ICommand
{
    private IMessageRepository _messageRepository;
    private IUserRepository _userRepository;
    private static readonly object QueryLock = new();
    
    public CommandResult Execute(CommandQuery? query, Menu menu, string? authUsername = null)
    {
        CommandQuery? cmdQuery;
        _messageRepository = MessageRepository.Instance;
        _userRepository = UserRepository.Instance;
        
        User? receiverUser;
        List<Message> messages = new List<Message>();

        lock (QueryLock)
        {
            receiverUser = _userRepository.QueryByUsername(authUsername);
            messages = _messageRepository.GetUnreadMessagesForUser(receiverUser.Id);   
        }

        messages.Sort(new MessageDateComparer());
        
        if (messages.Count > 0)
        {
            
            var unreadMessages = $"\n{messages.Aggregate("", (menuString, item) => menuString + item + "\n")}";
            messages.ForEach(m => m.Seen = true);
            
            cmdQuery = new CommandQuery(
                new Dictionary<string, string>
                {
                    {"RESULT", $"{unreadMessages}"},
                    {"MENU", $"{menu}"},
                }
            );
            return new CommandResult(cmdQuery);
            
        }
        cmdQuery = new CommandQuery(
            new Dictionary<string, string>
            {
                {"RESULT", "You don't have unread messages"},
                {"MENU", $"{menu}"},
            }
        );
        return new CommandResult(cmdQuery);
    }
}