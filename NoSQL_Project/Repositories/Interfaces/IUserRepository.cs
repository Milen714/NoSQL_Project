using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;

namespace NoSQL_Project.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetActiveUsers(string searchString, UserType userTypeFilter, bool hasType);
        Task Add(User user);

        public Task<User> FindByIdAsync(string id);
        public Task<User> GetUserByEmailAsync(string email);
        Task UpdateUserAsync(User user);

        Task<User> FindUserByNameAsync(string firstName, string lastName);



	}
}




