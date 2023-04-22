using F1.Constants;
using F1.Domain.Model;
using F1.Domain.Repository;
using F1.Presentation.Views.Menu;

namespace Server.Commands;

public class AddPartCommand : ICommand
{
    private IPartRepository _partRepository;
    public CommandResult Execute(CommandQuery? query, Menu menu)
    {
        CommandQuery? cmdQuery;
        if (query == null)
        {
            cmdQuery = new CommandQuery(new Dictionary<string, string>
            {
                {ConstantKeys.PartNameKey, "Enter"},
                {ConstantKeys.PartSupplierKey, "Enter"},
                {ConstantKeys.PartBrandKey, "Enter"}
            });
            
            return new CommandResult(cmdQuery);
        }
        
        _partRepository = PartRepository.Instance;
        query.Fields.TryGetValue(ConstantKeys.PartNameKey, out var name);
        query.Fields.TryGetValue(ConstantKeys.PartBrandKey, out var brand);
        query.Fields.TryGetValue(ConstantKeys.PartSupplierKey, out var supplier);

        _partRepository.AddPart(new Part(name, supplier, brand));
        
        cmdQuery = new CommandQuery(
            new Dictionary<string, string>
            {
                {"RESULT", "Part added correctly"},
                {"MENU", $"{menu}"}
            }
        );
        
        return new CommandResult(cmdQuery); 
    }
}