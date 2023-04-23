using F1.Domain.Comparator;
using F1.Domain.Repository;
using F1.Presentation.Views.Menu;

namespace Server.Commands;

public class MessageHistoryCommand : ICommand
{
    private IMessageRepository _messageRepository;
    private IUserRepository _userRepository;
    public CommandResult Execute(CommandQuery? query, Menu menu, string? authUsername = null)
    {
        CommandQuery? cmdQuery;
        _messageRepository = MessageRepository.Instance;
        _userRepository = UserRepository.Instance;
        
        var receiverUser = _userRepository.QueryByUsername(authUsername);
        var messages = _messageRepository.GetAllMessagesForUser(receiverUser.Id);
        
        messages.Sort(new MessageDateComparer());
        
        if (messages.Count > 0)
        {
            var history = $"\n{messages.Aggregate("", (menuString, item) => menuString + item + "\n")}";
                
            cmdQuery = new CommandQuery(
                new Dictionary<string, string>
                {
                    {"RESULT", $"{history}"},
                    {"MENU", $"{menu}"}
                }
            );
            return new CommandResult(cmdQuery);
            
        }
        cmdQuery = new CommandQuery(
            new Dictionary<string, string>
            {
                {"RESULT", "You don't have any messages"},
                {"MENU", $"{menu}"},
            }
        );
        return new CommandResult(cmdQuery);
    }
}