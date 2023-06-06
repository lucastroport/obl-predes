using F1.Constants;
using F1.Domain.Model;
using F1.Domain.Repository;
using F1.Presentation.Views.Menu;
using Server.Logging;

namespace Server.Commands;

public class QueryPartsCommand : ICommand
{
    private RabbitMQLogger rabbitMqLogger;
    private IPartRepository _partRepository;
    private static readonly object QueryContainsNameLock = new();
    private static readonly object QueryItemsByNameLock = new();
    public CommandResult Execute(CommandQuery? query, Menu menu, string? authUsername)
    {
        rabbitMqLogger = new RabbitMQLogger(
            LoggingConfigValues.QueueHost, 
            LoggingConfigValues.QueueUsername,
            LoggingConfigValues.QueuePassword,
            LoggingConfigValues.ExchangeName);
        
        _partRepository = PartRepository.Instance;
        CommandQuery? cmdQuery;

        if (query == null)
        {
            cmdQuery = new CommandQuery(new Dictionary<string, string>
            {
                {$"{ConstantKeys.SearchPartKey}", "Enter"}
            });
            rabbitMqLogger.Dispose();
            return new CommandResult(cmdQuery);
        }

        bool containsNameSearch;
        string searchName = "";
        lock (QueryContainsNameLock)
        {
            containsNameSearch= query.Fields.TryGetValue(ConstantKeys.SearchPartKey, out searchName);   
        }

        if (containsNameSearch)
        {
            List<Part> found = new List<Part>();
            lock (QueryItemsByNameLock)
            {
                found = _partRepository.QueryItemsByName(searchName);   
            }

            if (found.Count > 0)
            {
                var parts = $"\n{found.Aggregate("", (menuString, item) => menuString + item + "\n")}";
                
                cmdQuery = new CommandQuery(
                    new Dictionary<string, string>
                    {
                        {"RESULT", $"{parts}"},
                        {"MENU", $"{menu}"},
                    }
                );
                rabbitMqLogger.LogInfo($" (USER: {authUsername}) parts found: {parts}");
                rabbitMqLogger.Dispose();
                return new CommandResult(cmdQuery);
            }
        }
        rabbitMqLogger.LogInfo($" (USER: {authUsername}) The part with name {searchName} doesn't return any matches");
        rabbitMqLogger.Dispose();
        return new CommandResult(new CommandQuery(
            new Dictionary<string, string>
            {
                {"RESULT", $"ERROR: The part with name {searchName} doesn't return any matches"},
                {"MENU", $"{menu}"}
            }
        ));
    }
}