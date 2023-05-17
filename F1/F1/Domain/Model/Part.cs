namespace F1.Domain.Model;

public class Part
{
    public string Id { get; set; }
    public string Name { get; }
    public string? Supplier { get; }
    public string? Brand { get; }
    public string? PhotoUrl { get; set; }
    public List<PartCategory>? Categories { get; }

    public Part(string name, string supplier, string brand)
    {
        Name = name;
        Supplier = supplier;
        Brand = brand;
        Categories = new List<PartCategory>();
    }
    protected bool Equals(Part other)
    {
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Part)obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override string ToString() => $"({Id}) - Name: {Name} Supplier: {Supplier} Brand: {Brand} Categories: ({String.Join(", ", Categories.Select(c => c.Name))})";
}