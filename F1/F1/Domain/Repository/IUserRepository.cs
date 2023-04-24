using F1.Domain.Model;

namespace F1.Domain.Repository;

public interface IUserRepository
{
    void AddUser(User u);
    User? QueryByUsername(string username);
    User? QueryUserById(string id);
    List<User> GetAllMechanicUsers();
}