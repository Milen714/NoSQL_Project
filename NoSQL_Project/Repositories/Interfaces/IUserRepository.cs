using NoSQL_Project.Models;

namespace NoSQL_Project.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetAll();
        Task Add(User user);

        public Task<User> FindByIdAsync(string id);
        public Task<User> GetUserByEmailAsync(string email);
        Task UpdateUserAsync(User user);

        Task<User> FindUserByNameAsync(string firstName, string lastName);



	}
}




