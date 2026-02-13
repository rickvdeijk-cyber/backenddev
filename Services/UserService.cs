using UserManagementAPI.Models;

namespace UserManagementAPI.Services;

public class UserService
{
    private readonly List<User> _users = new()
    {
        new User { Id = 1, Name = "John Doe", Email = "john.doe@techhive.com", Department = "IT" },
        new User { Id = 2, Name = "Jane Smith", Email = "jane.smith@techhive.com", Department = "HR" }
    };

    private int _nextId = 3;

    public IEnumerable<User> GetAllUsers() => _users;

    public User? GetUserById(int id) => _users.FirstOrDefault(u => u.Id == id);

    public User CreateUser(User user)
    {
        user.Id = _nextId++;
        _users.Add(user);
        return user;
    }

    public bool UpdateUser(int id, User updatedUser)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null) return false;

        if (updatedUser.Id != id && updatedUser.Id != 0) return false; // Prevent ID change

        user.Name = updatedUser.Name ?? user.Name;
        user.Email = updatedUser.Email ?? user.Email;
        user.Department = updatedUser.Department ?? user.Department;
        return true;
    }

    public bool DeleteUser(int id)
    {
        var user = _users.FirstOrDefault(u => u.Id == id);
        if (user == null) return false;
        _users.Remove(user);
        return true;
    }
}