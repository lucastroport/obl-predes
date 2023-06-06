using F1.Constants;
using F1.Domain.Model;
using F1.Domain.Repository;
using F1.Presentation.Views.Menu;
using Server.Logging;

namespace Server.Commands;

public class AddPartCommand : ICommand
{
    private IPartRepository _partRepository;
    private RabbitMqLogger rabbitMqLogger;
    private static readonly object PartAddLock = new();
    public CommandResult Execute(CommandQuery? query, Menu menu, string? authUsername)
    {
        rabbitMqLogger = new RabbitMqLogger(
            LoggingConfigValues.QueueHost, 
            LoggingConfigValues.QueueUsername,
            LoggingConfigValues.QueuePassword,
            LoggingConfigValues.ExchangeName);
        
        CommandQuery? cmdQuery;
        if (query == null)
        {
            cmdQuery = new CommandQuery(new Dictionary<string, string>
            {
                {ConstantKeys.PartNameKey, "Enter"},
                {ConstantKeys.PartSupplierKey, "Enter"},
                {ConstantKeys.PartBrandKey, "Enter"}
            });
            rabbitMqLogger.Dispose();
            return new CommandResult(cmdQuery);
        }
        
        _partRepository = PartRepository.Instance;
        query.Fields.TryGetValue(ConstantKeys.PartNameKey, out var name);
        query.Fields.TryGetValue(ConstantKeys.PartBrandKey, out var brand);
        query.Fields.TryGetValue(ConstantKeys.PartSupplierKey, out var supplier);

        lock (PartAddLock)
        {
            var part = new Part(name, supplier, brand);
            _partRepository.AddPart(part);
            rabbitMqLogger.LogPartInfo($" (USER: {authUsername}) Part Added : {part}");
        }

        cmdQuery = new CommandQuery(
            new Dictionary<string, string>
            {
                {"RESULT", "Part added correctly"},
                {"MENU", $"{menu}"}
            }
        );
        rabbitMqLogger.Dispose();
        return new CommandResult(cmdQuery); 
    }
}