using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;

namespace NoSQL_Project.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<User>> GetActiveUsers(string searchString, UserType userTypeFilter, bool hasType);
        Task Add(User user);
        public Task<User> FindByIdAsync(string id);
        public Task<User> GetUserByEmailAsync(string email);
        public Task<string> GeneratePasswordResetTokenAsync(User user);
        public Task<User> AuthenticateUserAsync(LoginModel model);
        public User HashUserPassword(User user);
        Task UpdateUserAsync(User user);

		Task<User> FindUserByNameAsync(string firstName, string lastName);


	}
}
