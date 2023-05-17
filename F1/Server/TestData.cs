using F1.Domain.Model;

namespace Server;

public static class TestData
{
    public static List<Part> GetDummyParts()
    {
        return new ()
        {
            new Part("Engine", "Toyota", "BXZ1"),
            new Part("Wheel", "Ford", "AC123"),
            new Part("Paint", "BMW", "ZX789"),
            new Part("Brake", "Honda", "Fusion"),
            new Part("Transmission", "Chevrolet", "XYZ456")
        };
    }
    
    public static List<PartCategory> GetDummyPartCategories()
    {
        return new ()
        {
            new PartCategory("Engines"),
            new PartCategory("Wheels"),
            new PartCategory("Paints"),
            new PartCategory("Brakes"),
            new PartCategory("Transmissions")
        };
    }
    
    public static List<User> GetDummyUsers()
    {
        return new ()
        {
            new User("johnsmith", "Pa$$w0rd", UserType.Admin),
            new User("janedoe", "Secur3P@ss"),
            new User("michaelsullivan", "Passw0rd!"),
            new User("laurawilson", "P@ssword123")
        };
    }
    
    
}