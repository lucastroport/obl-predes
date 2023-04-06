namespace F1.Domain.Model;

public class Part
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? Supplier { get; set; }
    public string? Brand { get; set; }
    public string? PhotoUrl { get; set; }
    public IList<PartCategory>? Categories { get; set; }

    public Part(string id, string name)
    {
        Id = id;
        Name = name;
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
}