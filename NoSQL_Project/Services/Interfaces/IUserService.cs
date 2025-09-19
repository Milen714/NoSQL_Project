using NoSQL_Project.Models;

namespace NoSQL_Project.Services.Interfaces
{
    public interface IUserService
    {
        List<User> GetAll();
        void Add(User user);
        public User FindById(string id);
        public User GetUserByEmail(string email);
        public Task<string> GeneratePasswordResetTokenAsync(User user);
        public User AuthenticateUser(LoginModel model);
        public User HashUserPassword(User user);
        Task UpdateUser(User user);



        // Future:
        // User? GetById(string id);
        // void Update(User user);
        // void Delete(string id);
    }
}
