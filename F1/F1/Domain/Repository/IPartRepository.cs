using F1.Domain.Model;

namespace F1.Domain.Repository;

public interface IPartRepository
{
    void AddPart(Part p);
    void RemovePart(Part p);
    Part? QueryById(string id);
    List<Part> QueryItemsByName(string name);
    List<Part> GetAllParts();
}