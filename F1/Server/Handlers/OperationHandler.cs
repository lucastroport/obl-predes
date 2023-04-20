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
                // { MenuItemConstants.AddUser, new AddUserCommand() },
                // { MenuItemConstants.AddPart, new AddPartCommand() },
                // { MenuItemConstants.AddPartCategory, new AddPartCategoryCommand() },
                // { MenuItemConstants.AssociateCategoryToPart, new AssociateCategoryToPartCommand() },
                // { MenuItemConstants.AssociatePictureToPart, new AssociatePictureToPartCommand() },
                // { MenuItemConstants.SearchPartById, new SearchPartByIdCommand() },
                // { MenuItemConstants.SearchPartByName, new SearchPartByNameCommand() },
                // { MenuItemConstants.Chat, new ChatCommand() },
                // { MenuItemConstants.History, new HistoryCommand() },
                // { MenuItemConstants.ResetAdminPassword, new ResetAdminPasswordCommand() },
                // { MenuItemConstants.ChangeIpPort, new ChangeIpPortCommand() },
                // { MenuItemConstants.ChangeIpAddress, new ChangeIpAddressCommand() },
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