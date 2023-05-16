using F1.Constants;
using F1.Presentation.Views.Menu;
using ProtocolHelper.Communication.Models;
using Server.Commands;

namespace Server.Handlers;

public class OperationHandler
    {
        private readonly Dictionary<int, ICommand> commandMap;
        public OperationHandler()
        {
            commandMap = new ()
            {
                { MenuItemConstants.LogIn, new LoginCommand() },
                { MenuItemConstants.AddUser, new AddUserCommand() },
                { MenuItemConstants.LogOut, new LogOutCommand() },
                { MenuItemConstants.AddPart, new AddPartCommand() },
                { MenuItemConstants.Parts, new PartsCommand() },
                { MenuItemConstants.AddPartCategory, new AddCategoryCommand() },
                { MenuItemConstants.AssociateCategoryToPart, new LinkCategoryCommand() },
                { MenuItemConstants.AssociatePictureToPart, new LinkFileToPartCommand() },
                { MenuItemConstants.DownloadPartPicture, new DownloadPartPhotoCommand() },
                { MenuItemConstants.SearchPartByName, new QueryPartsCommand() },
                { MenuItemConstants.Messages, new MessagesCommand() },
                { MenuItemConstants.SendMessage, new SendMessageCommand() },
                { MenuItemConstants.UnreadMessages, new UnreadMessagesCommand() },
                { MenuItemConstants.History, new MessageHistoryCommand() },
                { MenuItemConstants.MainMenu, new MainMenuCommand() },
                { MenuItemConstants.LoadTestData, new LoadTestDataCommand() },
            };
        }

        public ProtocolData HandleMenuAction(int operation, CommandQuery? query, Menu menu, string? authUsername)
        {
            return HandleCommand(operation, query, menu, authUsername);
        }

        private ProtocolData HandleCommand(int operation, CommandQuery? query, Menu menu, string? authUsername)
        {
            if (commandMap.TryGetValue(operation, out ICommand command))
            {
                if (query == null && operation == 0)
                {
                    return new ProtocolData(
                        false,
                        $"{operation}",
                        new QueryData
                        {
                            Fields = new Dictionary<string, string>{ {"MENU", $"{menu}"} }
                        }
                    );
                }
                
                var result = command.Execute(query, menu, authUsername);
                    
                return new ProtocolData(
                    false,
                    $"{operation}",
                    new QueryData
                    {
                        Fields = result.CommandQuery.Fields
                    }
                );  
            }
            return new ProtocolData(
                false,
                $"{operation}",
                new QueryData
                {
                    Fields = new Dictionary<string, string>{ {"RESULT",$"ERROR: Invalid operation {operation}"} }
                }
            );
        }
    }