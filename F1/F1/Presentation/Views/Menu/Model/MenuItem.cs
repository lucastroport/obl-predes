using F1.Domain.Model;

namespace F1.Presentation.Views.Menu.Model;

public class MenuItem
{
    public int Id { get; }
    public string Name { get; }
    public bool Enabled { get; set; }
    public UserType[] Access { get; set; }
    
    public bool NeedsInput { get; set; }

    public MenuItem(int id, string name, UserType[] access, bool needsInput = false)
    {
        Enabled = true;
        Name = name;
        Id = id;
        Access = access;
        NeedsInput = needsInput;
    }

    public override string ToString()
    {
        return $"{Id} - {Name}";
    }
}