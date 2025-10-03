using NoSQL_Project.Models;

namespace NoSQL_Project.Repositories.Interfaces
{
    public interface IUserRepository
    {
        List<User> GetAll();
        void Add(User user);
        
        public User FindById(string id);
        public User GetUserByEmail(string email);
        public User HashUserPassword(User user);
        public User AuthenticateUser(LoginModel model);
        Task UpdateUser(User user);

        // Future:
        // User? GetById(string id);
        // void Update(User user);
        // void Delete(string id);


	}
}




