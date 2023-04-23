using F1.Domain.Model;

namespace F1.Domain.Repository;

public interface ICategoryRepository
{
    void AddPartCategory(PartCategory category);
    PartCategory? QueryCategoryById(string id);
    PartCategory? QueryCategoryByName(string name);
    List<PartCategory> GetAllCategories();
}