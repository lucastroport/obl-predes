using F1.Constants;
using F1.Domain.Model;
using F1.Domain.Repository;
using F1.Presentation.Views.Menu;

namespace Server.Commands;

public class LinkCategoryCommand : ICommand
{
    private IPartRepository _partRepository;
    private ICategoryRepository _categoryRepository;
    private static readonly object PartQueryByIdLock = new();
    private static readonly object CategoryQueryByIdLock = new();
    private static readonly object AddCategoryLock = new();
    private static readonly object QueryLock = new();
    public CommandResult Execute(CommandQuery? query, Menu menu, string? authUsername)
    {
        lock (QueryLock)
        {
            _partRepository = PartRepository.Instance;
            _categoryRepository = CategoryRepository.Instance;   
        }
        CommandQuery? cmdQuery;
        
        if (query == null)
        {
            var parts = _partRepository.GetAllParts();
            var categories = _categoryRepository.GetAllCategories();
            
            if (parts.Count == 0)
            {
                cmdQuery = new CommandQuery(
                    new Dictionary<string, string>
                    {
                        {"RESULT", "There are no parts to link category, please add a part first."},
                        {"MENU", $"{menu}"}
                        
                    }
                );
                return new CommandResult(cmdQuery);
            }

            if (categories.Count == 0)
            {
                cmdQuery = new CommandQuery(
                    new Dictionary<string, string>
                    {
                        {"RESULT", "There are no categories to link, please add a category first."},
                        {"MENU", $"{menu}"}
                    }
                );
                return new CommandResult(cmdQuery);
            }
            
            cmdQuery = new CommandQuery(
                new Dictionary<string, string>
                {
                    {$"{ConstantKeys.SelectPartKey}", $"\n{parts.Aggregate("", (menuString, item) => menuString + item + "\n")}"},
                    {$"{ConstantKeys.SelectPartCategoryKey}", $"\n{categories.Aggregate("", (menuString, item) => menuString + item + "\n")}"}
                }
            );
            
            return new CommandResult(cmdQuery);
        }
        
        query.Fields.TryGetValue(ConstantKeys.SelectPartKey, out var partId);
        query.Fields.TryGetValue(ConstantKeys.SelectPartCategoryKey, out var categoryId);

        Part? foundPart;
        lock (PartQueryByIdLock)
        {
            foundPart = _partRepository.QueryById(partId);
        }

        PartCategory? foundCategory;
        lock (CategoryQueryByIdLock)
        {
            foundCategory = _categoryRepository.QueryCategoryById(categoryId);   
        }
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
            lock (AddCategoryLock)
            {
                foundPart.Categories.Add(foundCategory);   
            }
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