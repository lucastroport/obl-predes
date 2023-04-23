using F1.Constants;
using F1.Domain.Repository;
using F1.Presentation.Views.Menu;

namespace Server.Commands;

public class QueryPartsCommand : ICommand
{
    private IPartRepository _partRepository;
    public CommandResult Execute(CommandQuery? query, Menu menu)
    {
        _partRepository = PartRepository.Instance;
        
        CommandQuery? cmdQuery;
        _partRepository = PartRepository.Instance;
        
        if (query == null)
        {
            cmdQuery = new CommandQuery(new Dictionary<string, string>
            {
                {$"{ConstantKeys.SearchPartKey}", "Enter"}
            });
            return new CommandResult(cmdQuery);
        }
        
        var containsNameSearch= query.Fields.TryGetValue(ConstantKeys.SearchPartKey, out var searchName);
        
        if (containsNameSearch)
        {
            var found = _partRepository.QueryItemsByName(searchName);

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
                return new CommandResult(cmdQuery);
            }
        }
        
        return new CommandResult(new CommandQuery(
            new Dictionary<string, string>
            {
                {"RESULT", $"ERROR: The part with name {searchName} doesn't return any matches"},
                {"MENU", $"{menu}"}
            }
        ));
    }
}