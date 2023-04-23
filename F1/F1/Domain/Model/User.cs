namespace F1.Domain.Model;

public class User
{
    public string Id { get; set; }
    public string Username { get; }
    public string Password { get; }
    public UserType Type { get; set; }
    public bool IsLoggedIn { get; set; }

    public User(string username, string password, UserType userType = UserType.Mechanic)
    {
        Username = username;
        Password = password;
        Type = userType;
        IsLoggedIn = false;
    }

    protected bool Equals(User other)
    {
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((User)obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override string ToString()
    {
        return $"({Id}) - {Username}";
    }
}