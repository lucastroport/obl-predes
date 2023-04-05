using F1.Domain.Model;

namespace F1.Domain.Repository;

public class PartRepository : IPartRepository
{
    private IList<Part> _parts;

    public PartRepository()
    {
        _parts = new List<Part>();
    }
    
    public void AddPart(Part p)
    {
        if (_parts.Contains(p))
        {
            throw new ArgumentException($"Cannot add Part with duplicated id: {p.Id}");
        }
        _parts.Add(p);
    }

    public void RemovePart(Part p)
    {
        _parts.Remove(p);
    }

    public Part? QueryById(string id)
    {
        return _parts.FirstOrDefault(p => p.Id == id);
    }

    public IList<Part> QueryItemsByName(string name)
    {
        return _parts.TakeWhile(p => p.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
    }
}