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
            commandMap = new Dictionary<int, ICommand>
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
            };
        }

        public ProtocolData HandleMenuAction(int operation, CommandQuery? query, Menu menu)
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
            return HandleCommand(operation, query, menu);
        }

        private ProtocolData HandleCommand(int operation, CommandQuery? query, Menu menu)
        {
            if (commandMap.TryGetValue(operation, out ICommand command))
            {
                var result = command.Execute(query, menu);
                
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
                    Fields = new Dictionary<string, string>{ {"RESULT",$"Invalid operation {operation}"} }
                }
            );
        }
    }