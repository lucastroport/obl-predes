using F1.Domain.Model;

namespace F1.Domain.Repository;

public class UserRepository : IUserRepository
{
    private IList<User> _users;

    public UserRepository()
    {
        _users = new List<User>();
    }
    
    public void AddUser(User u)
    {
        u.Id = $"{_users.Count+1}";
        _users.Add(u);
    }

    public void RemoveUser(User u)
    {
        _users.Remove(u);
    }

    public User? QueryById(string id)
    {
        return _users.FirstOrDefault(user => user.Id == id);
    }
}