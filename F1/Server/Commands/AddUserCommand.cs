using System.Reflection.Metadata;
using F1.Constants;
using F1.Domain.Model;
using F1.Domain.Repository;
using F1.Presentation.Views.Menu;

namespace Server.Commands;

public class AddUserCommand : ICommand
{
    private IUserRepository _userRepository;
    private static readonly object UserAddLock = new();
    private static readonly object UserQueryLock = new();
    public CommandResult Execute(CommandQuery? query, Menu menu, string? authUsername)
    {
        CommandQuery? cmdQuery;
        if (query == null)
        {
            cmdQuery = new CommandQuery(new Dictionary<string, string>
            {
                {ConstantKeys.UsernameKey, "Enter"},
                {ConstantKeys.PasswordKey, "Enter"}
            });
            return new CommandResult(cmdQuery);
        }
        _userRepository = UserRepository.Instance;
        query.Fields.TryGetValue(ConstantKeys.UsernameKey, out var username);
        query.Fields.TryGetValue(ConstantKeys.PasswordKey, out var password);

        User? authenticatedUser = null;
        bool userExists;
        
        lock (UserQueryLock)
        {
            authenticatedUser = _userRepository.QueryByUsername(authUsername);
            userExists = _userRepository.QueryByUsername(username) != null;   
        }

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
                lock (UserAddLock)
                {
                    _userRepository.AddUser(
                        new User(username, password)
                    );   
                }

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