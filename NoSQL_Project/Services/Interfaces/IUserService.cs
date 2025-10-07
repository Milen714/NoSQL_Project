using NoSQL_Project.Models;

namespace NoSQL_Project.Services.Interfaces
{
    public interface IUserService
    {
        List<User> GetAll();
        void Add(User user);
        public Task<User> FindByIdAsync(string id);
        public Task<User> GetUserByEmailAsync(string email);
        public Task<string> GeneratePasswordResetTokenAsync(User user);
        public Task<User> AuthenticateUserAsync(LoginModel model);
        public User HashUserPassword(User user);
        Task UpdateUserAsync(User user);



        // Future:
        // User? GetById(string id);
        // void Update(User user);
        // void Delete(string id);
    }
}
