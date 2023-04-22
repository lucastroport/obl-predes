namespace F1.Domain.Model;

public class PartCategory
{
    public string Id { get; set; }
    public string Name { get; }

    public PartCategory(string name)
    {
        Name = name;
    }

    public override string ToString()
    {
        return $"({Id}) - {Name}";
    }
}