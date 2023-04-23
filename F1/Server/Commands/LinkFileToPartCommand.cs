using F1.Constants;
using F1.Domain.Model;
using F1.Domain.Repository;
using F1.Presentation.Views.Menu;

namespace Server.Commands;

public class LinkFileToPartCommand : ICommand
{
    private static readonly object QueryLock = new(); 
    private IPartRepository _partRepository;
    public CommandResult Execute(CommandQuery? query, Menu menu, string? authUsername)
    {
        CommandQuery? cmdQuery;
        _partRepository = PartRepository.Instance;
        
        List<Part> parts = new List<Part>();
        lock (QueryLock)
        {
            parts = _partRepository.GetAllParts();
        }
        
        if (parts.Count == 0)
        {
            cmdQuery = new CommandQuery(
                new Dictionary<string, string>
                {
                    {"RESULT", "There are no parts to link picture, please add a part first."},
                    {"MENU", $"{menu}"}
                }
            );
            return new CommandResult(cmdQuery);
        }
        
        if (query == null)
        {
            cmdQuery = new CommandQuery(new Dictionary<string, string>
            {
                {$"{ConstantKeys.SelectPartKey}", $"\n{parts.Aggregate("", (menuString, item) => menuString + item + "\n")}"},
                {ConstantKeys.EnterFilePathKey, "Enter"}
            });
            return new CommandResult(cmdQuery);
        }
        
        query.Fields.TryGetValue(ConstantKeys.EnterFilePathKey, out var path);
        query.Fields.TryGetValue(ConstantKeys.SelectPartKey, out var partId);

        return new CommandResult(
            new CommandQuery(
                new Dictionary<string, string>
                {
                    {ConstantKeys.SaveFileKey, path},
                    {ConstantKeys.PartKey, partId}
                }
            )
        );
    }
}