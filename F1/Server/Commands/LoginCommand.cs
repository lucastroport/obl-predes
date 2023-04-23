using System.Runtime.InteropServices;
using F1.Constants;
using F1.Domain.Model;
using F1.Domain.Repository;
using F1.Presentation.Views.Menu;

namespace Server.Commands;

public class LoginCommand : ICommand
{
    private IUserRepository _userRepository;
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

        var foundUser = _userRepository.QueryByUsername(username);
        string resultMessage = "User or password incorrect";
        
        if (foundUser != null)
        {
            if (password.Equals(foundUser.Password))
            {
                foundUser.IsLoggedIn = true;
                resultMessage = "Login successful";
                menu.TriggerLoggedInMenu();
            }
        }
        cmdQuery = new CommandQuery(
            new Dictionary<string, string>
            {
                {"RESULT", $"{resultMessage}"},
                {"MENU", $"{menu}"}
            }
            );
        if (foundUser != null)
        {
            cmdQuery.Fields.Add("AUTHENTICATED", foundUser.Username);
        }
        return new CommandResult(cmdQuery);
    }
}