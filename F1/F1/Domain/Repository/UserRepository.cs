using F1.Domain.Model;

namespace F1.Domain.Repository
{
    public class UserRepository : IUserRepository
    {
        private IList<User> _users;
        private static UserRepository _instance;

        private UserRepository()
        {
            _users = new List<User>();
            _users.Add(new User("admin","admin", UserType.Admin));
        }
        public static UserRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UserRepository();
                }
                return _instance;
            }
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

        public User? QueryByUsername(string username)
        {
            return _users.FirstOrDefault(user => user.Username == username);
        }
        
    }
}