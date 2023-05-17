using F1.Domain.Model;

namespace F1.Presentation.Views.Menu.Model;

public class MenuItem
{
    public int Id { get; }
    public string Name { get; }
    public bool Enabled { get; set; }

    public MenuItem(int id, string name, UserType[] access, bool needsInput = false)
    {
        Enabled = true;
        Name = name;
        Id = id;
    }

    public override string ToString()
    {
        return $"{Id} - {Name}";
    }
}