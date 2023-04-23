using F1.Constants;
using F1.Domain.Model;
using F1.Domain.Repository;
using F1.Presentation.Views.Menu;

namespace Server.Commands;

public class AddCategoryCommand : ICommand
{
    private ICategoryRepository _categoryRepository;
    private static readonly object CategoryAddLock = new();
    private static readonly object CategoryQueryLock = new();
    public CommandResult Execute(CommandQuery? query, Menu menu, string? authUsername)
    {
        CommandQuery? cmdQuery;
        if (query == null)
        {
            cmdQuery = new CommandQuery(new Dictionary<string, string>
            {
                {ConstantKeys.CategoryNameKey, "Enter"}
            });
            
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
            }
        }
        return new CommandResult(cmdQuery);
    }
    
}