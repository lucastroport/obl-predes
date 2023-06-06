using F1.Constants;
using F1.Domain.Model;
using F1.Domain.Repository;
using F1.Presentation.Views.Menu;
using Server.Logging;

namespace Server.Commands;

public class AddCategoryCommand : ICommand
{
    private ICategoryRepository _categoryRepository;
    private RabbitMQLogger rabbitMqLogger;
    private static readonly object CategoryAddLock = new();
    private static readonly object CategoryQueryLock = new();
    public CommandResult Execute(CommandQuery? query, Menu menu, string? authUsername)
    {
        rabbitMqLogger = new RabbitMQLogger(
            LoggingConfigValues.QueueHost, 
            LoggingConfigValues.QueueUsername,
            LoggingConfigValues.QueuePassword,
            LoggingConfigValues.ExchangeName);
        
        CommandQuery? cmdQuery;
        if (query == null)
        {
            cmdQuery = new CommandQuery(new Dictionary<string, string>
            {
                {ConstantKeys.CategoryNameKey, "Enter"}
            });
            
            rabbitMqLogger.Dispose();
            return new CommandResult(cmdQuery);
        }
        
        _categoryRepository = CategoryRepository.Instance;
        query.Fields.TryGetValue(ConstantKeys.CategoryNameKey, out var name);
        bool alreadyExists;
        lock (CategoryQueryLock)
        {
            alreadyExists = _categoryRepository.QueryCategoryByName(name) != null;   
        }

        if (alreadyExists)
        {
            rabbitMqLogger.LogClientError($" (USER: {authUsername}) Category {name} already exists");
            cmdQuery = new CommandQuery(
                new Dictionary<string, string>
                {
                    {"RESULT", "ERROR: Category already exists."},
                    {"MENU", $"{menu}"}
                }
            ); 
        }
        else
        {
            lock (CategoryAddLock)
            {
                _categoryRepository.AddPartCategory(
                    new PartCategory(name)
                );
                cmdQuery = new CommandQuery(
                    new Dictionary<string, string>
                    {
                        {"RESULT", "Category successfully added"},
                        {"MENU", $"{menu}"}
                    }
                );
                rabbitMqLogger.LogInfo($" (USER: {authUsername}) Category Added : {name}");
            }
        }
        rabbitMqLogger.Dispose();
        return new CommandResult(cmdQuery);
    }
    
}