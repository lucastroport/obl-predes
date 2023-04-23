using F1.Constants;
using F1.Domain.Model;
using F1.Domain.Repository;
using F1.Presentation.Views.Menu;

namespace Server.Commands;

public class LinkCategoryCommand : ICommand
{
    private IPartRepository _partRepository;
    private ICategoryRepository _categoryRepository;
    public CommandResult Execute(CommandQuery? query, Menu menu)
    {
        _partRepository = PartRepository.Instance;
        _categoryRepository = CategoryRepository.Instance;
        CommandQuery? cmdQuery;
        
        if (query == null)
        {
             cmdQuery = new CommandQuery(
                new Dictionary<string, string>
                {
                    {$"{ConstantKeys.SelectPartKey}", $"\n{_partRepository.GetAllParts().Aggregate("", (menuString, item) => menuString + item + "\n")}"},
                    {$"{ConstantKeys.SelectPartCategoryKey}", $"\n{_categoryRepository.GetAllCategories().Aggregate("", (menuString, item) => menuString + item + "\n")}"}
                }
            );
             return new CommandResult(cmdQuery);
        }
        
        query.Fields.TryGetValue(ConstantKeys.SelectPartKey, out var partId);
        query.Fields.TryGetValue(ConstantKeys.SelectPartCategoryKey, out var categoryId);

        var foundPart = _partRepository.QueryById(partId);
        var foundCategory = _categoryRepository.QueryCategoryById(categoryId);
        var resultMessage = "";
        if (foundCategory == null)
        {
            resultMessage = "Category selected is incorrect";
        }

        if (foundPart == null)
        {
            resultMessage += " Part selected is incorrect";
        }

        if (foundCategory != null && foundPart != null)
        {
            foundPart.Categories.Add(foundCategory);
            resultMessage = $"{foundCategory.Name} category linked to {foundPart.Name}";
        }
        
        cmdQuery = new CommandQuery(
            new Dictionary<string, string>
            {
                {"RESULT", resultMessage},
                {"MENU", $"{menu}"}
            }
        );
        
        return new CommandResult(cmdQuery);
    }
}