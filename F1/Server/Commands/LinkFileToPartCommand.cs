using F1.Constants;
using F1.Domain.Repository;
using F1.Presentation.Views.Menu;

namespace Server.Commands;

public class LinkFileToPartCommand : ICommand
{
    private IPartRepository _partRepository;
    public CommandResult Execute(CommandQuery? query, Menu menu)
    {
        _partRepository = PartRepository.Instance;
        CommandQuery? cmdQuery;
        if (query == null)
        {
            cmdQuery = new CommandQuery(new Dictionary<string, string>
            {
                {$"{ConstantKeys.SelectPartKey}", $"\n{_partRepository.GetAllParts().Aggregate("", (menuString, item) => menuString + item + "\n")}"},
                {ConstantKeys.EnterFilePathKey, "Enter"}
            });
            return new CommandResult(cmdQuery);
        }
        
        query.Fields.TryGetValue(ConstantKeys.EnterFilePathKey, out var path);
        query.Fields.TryGetValue(ConstantKeys.SelectPartKey, out var partId);
        var part = _partRepository.QueryById(partId);

        if (part != null)
        {
            part.PhotoUrl = path;
        }
        
        return new CommandResult(
            new CommandQuery(
                new Dictionary<string, string>
                {
                    {ConstantKeys.SaveFileKey, path}
                }
            )
        );
    }
}