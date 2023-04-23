using F1.Domain.Model;

namespace F1.Domain.Repository;

public class CategoryRepository : ICategoryRepository
{
    private List<PartCategory> _categories;
    private static CategoryRepository _instance;
    
    private CategoryRepository()
    {
        _categories = new List<PartCategory>();
    }

    public static CategoryRepository Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new CategoryRepository();
            }
            return _instance;
        }
    }
    
    public void AddPartCategory(PartCategory category)
    {
        category.Id = $"{_categories.Count + 1}";
        _categories.Add(category);
    }

    public PartCategory? QueryCategoryById(string id)
    {
        return _categories.FirstOrDefault(p => p.Id == id);
    }

    public PartCategory? QueryCategoryByName(string name)
    {
        return _categories.FirstOrDefault(c => c.Name == name);
    }

    public List<PartCategory> GetAllCategories() => _categories;
}