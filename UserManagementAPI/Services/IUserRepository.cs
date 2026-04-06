using UserManagementAPI.Models;

namespace UserManagementAPI.Repositories
{
    public interface IUserRepository
    {
        IEnumerable<User> GetAll();
        User? GetById(int id);
        void Add(User user);
        void Update(User user);
        void Delete(int id);
    }

    public class InMemoryUserRepository : IUserRepository
    {
        private readonly List<User> _users = new();

        public IEnumerable<User> GetAll() => _users;

        public User? GetById(int id) => _users.FirstOrDefault(u => u.Id == id);

        public void Add(User user)
        {
            user.Id = _users.Count > 0 ? _users.Max(u => u.Id) + 1 : 1;
            _users.Add(user);
        }

        public void Update(User user)
        {
            var existing = GetById(user.Id);
            if (existing is null) return;
            existing.FirstName = user.FirstName;
            existing.LastName = user.LastName;
            existing.Email = user.Email;
            existing.Department = user.Department;
        }

        public void Delete(int id)
        {
            var user = GetById(id);
            if (user is not null) _users.Remove(user);
        }
    }
}