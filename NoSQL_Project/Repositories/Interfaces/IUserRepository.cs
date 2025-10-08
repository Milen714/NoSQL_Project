using NoSQL_Project.Models;

namespace NoSQL_Project.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetAll();
        void Add(User user);

        public Task<User> FindByIdAsync(string id);
        public Task<User> GetUserByEmailAsync(string email);
        public User HashUserPassword(User user);
        public Task<User> AuthenticateUserAsync(LoginModel model);
        Task UpdateUserAsync(User user);

        Task<User> FindUserByNameAsync(string firstName, string lastName);



	}
}




