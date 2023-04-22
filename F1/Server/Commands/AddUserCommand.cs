using System.Reflection.Metadata;
using F1.Constants;
using F1.Domain.Model;
using F1.Domain.Repository;
using F1.Presentation.Views.Menu;

namespace Server.Commands;

public class AddUserCommand : ICommand
{
    private IUserRepository _userRepository;
    public CommandResult Execute(CommandQuery? query, Menu menu)
    {
        CommandQuery? cmdQuery;
        if (query == null)
        {
            cmdQuery = new CommandQuery(new Dictionary<string, string>());
            cmdQuery.PopulateQuery(new List<string>{"user","password"});
            
            return new CommandResult(cmdQuery);
        }
        _userRepository = UserRepository.Instance;
        query.Fields.TryGetValue("user", out var username);
        query.Fields.TryGetValue("password", out var password);
        query.Fields.TryGetValue(ConstantKeys.Authenticated, out var authUsername);

        var authenticatedUser = _userRepository.QueryByUsername(authUsername);
        var userExists = _userRepository.QueryByUsername(username) != null;
        
        if (authenticatedUser != null && authenticatedUser.Type == UserType.Admin)
        {
            if (userExists)
            {
                cmdQuery = new CommandQuery(
                    new Dictionary<string, string>
                    {
                        {"RESULT", "ERROR: User already exists."},
                        {"MENU", $"{menu}"}
                    }
                ); 
            }
            else
            {
                _userRepository.AddUser(
                    new User(username, password)
                );
        
                cmdQuery = new CommandQuery(
                    new Dictionary<string, string>
                    {
                        {"RESULT", "User registered correctly."},
                        {"MENU", $"{menu}"}
                    }
                );   
            }
        }
        else
        {
            cmdQuery = new CommandQuery(
                new Dictionary<string, string>
                {
                    {"RESULT", "ERROR: You need admin access to add a user."},
                    {"MENU", $"{menu}"}
                }
            );  
        }
        return new CommandResult(cmdQuery);
    }
}