using F1.Domain.Model;
using F1.Domain.Repository;
using F1.Presentation.Views.Menu;
using Server.Logging;

namespace Server.Commands;

public class LoadTestDataCommand : ICommand
{
    private RabbitMqLogger rabbitMqLogger;
    private static readonly object QueryByUsernameLock = new();
    public CommandResult Execute(CommandQuery? query, Menu menu, string? authUsername = null)
    {
        rabbitMqLogger = new RabbitMqLogger(
            LoggingConfigValues.QueueHost, 
            LoggingConfigValues.QueueUsername,
            LoggingConfigValues.QueuePassword,
            LoggingConfigValues.ExchangeName);
        
        User? authenticatedUser;
        CommandQuery? cmdQuery;
        
        lock (QueryByUsernameLock)
        {
            authenticatedUser = UserRepository.Instance.QueryByUsername(authUsername);   
        }
        
        if (authenticatedUser is { Type: UserType.Mechanic })
        {
            cmdQuery = new CommandQuery(
                new Dictionary<string, string>
                {
                    {"RESULT", "ERROR: Test Data is available for Admin users."},
                    {"MENU", $"{menu}"}
                }
            );
            rabbitMqLogger.LogClientError($" (USER: {authUsername}) Test Data is available for Admin users.");
            rabbitMqLogger.Dispose();
            return new CommandResult(cmdQuery);
        }
        
        var cache = CacheManager.Instance;
        var testDataLoaded = cache.RetrieveConfigFlag(Constants.FLAG_TEST_DATA_LOADED);

        if (!testDataLoaded)
        {
            TestData.GetDummyUsers().ForEach(u => UserRepository.Instance.AddUser(u));
            TestData.GetDummyParts().ForEach(p => PartRepository.Instance.AddPart(p));
            TestData.GetDummyPartCategories().ForEach(pc => CategoryRepository.Instance.AddPartCategory(pc));
            
            cmdQuery = new CommandQuery(
                new Dictionary<string, string>
                {
                    {"MENU", $"{MenuOptions.ListItems(MenuOptions.MenuItems)}"},
                    {"RESULT", $"Test Data Loaded Correctly !" +
                               $"\n{TestData.GetDummyUsers().Count} Users loaded" +
                               $"\n {TestData.GetDummyParts().Count} Parts loaded" +
                               $"\n {TestData.GetDummyPartCategories().Count} Categories loaded"}
                }
            );
            cache.StoreConfig(Constants.FLAG_TEST_DATA_LOADED, true);
            rabbitMqLogger.LogInfo($" (USER: {authUsername}) Test Data Loaded Correctly !");
        }
        else
        {
            cmdQuery = new CommandQuery(
                new Dictionary<string, string>
                {
                    {"MENU", $"{MenuOptions.ListItems(MenuOptions.MenuItems)}"},
                    {"RESULT", "Test data has been already loaded"}
                }
            );
            rabbitMqLogger.LogInfo($" (USER: {authUsername}) Test data has been already loaded");
        }
        rabbitMqLogger.Dispose();
        return new CommandResult(cmdQuery);
    }
}