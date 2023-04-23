using F1.Constants;
using F1.Domain.Model;
using F1.Domain.Repository;
using F1.Presentation.Views.Menu;

namespace Server.Commands;

public class DownloadPartPhotoCommand : ICommand
{
    private IPartRepository _partRepository;
    private static readonly object QueryItemsByNameLock = new();
    private static readonly object QueryPartByIdLock = new();
    public CommandResult Execute(CommandQuery? query, Menu menu, string? authUsername)
    {
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
        
        query.Fields.TryGetValue(ConstantKeys.SelectPartKey, out var partId);
        var containsNameSearch= query.Fields.TryGetValue(ConstantKeys.SearchPartKey, out var searchName);

        if (containsNameSearch)
        {
            List<Part> found;

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
                        {$"{ConstantKeys.SelectPartKey}", $"{parts}"}
                    }
                );
                return new CommandResult(cmdQuery);
            }

            cmdQuery = new CommandQuery(
                new Dictionary<string, string>
                {
                    {"RESULT", "There are no parts to search, please add a part first"},
                    {"MENU", $"{menu}"}
                }
            );
            return new CommandResult(cmdQuery);
        }

        Part? part;
        lock (QueryPartByIdLock)
        {
            part = _partRepository.QueryById(partId);   
        }

        if (part != null && part.PhotoUrl != null)
        {
            cmdQuery = new CommandQuery(
                new Dictionary<string, string>
                {
                    {ConstantKeys.SendFileUrlKey, $"{part.PhotoUrl}"},
                    {"MENU", $"{menu}"}
                }
            );
            return new CommandResult(cmdQuery);
        }
        
        return new CommandResult(new CommandQuery(
            new Dictionary<string, string>
            {
                {"RESULT", $"ERROR: The part with name {searchName} doesn't exist or does not have a picture"},
                {"MENU", $"{menu}"}
            }
        ));
    }
}