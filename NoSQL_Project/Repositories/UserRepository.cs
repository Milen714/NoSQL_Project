using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using NoSQL_Project.Models;
using NoSQL_Project.Repositories.Interfaces;

namespace NoSQL_Project.Repositories
{

    public class UserRepository : IUserRepository
    {
        private readonly IMongoCollection<User> _users;

        public UserRepository(IMongoDatabase db)
        {
            _users = db.GetCollection<User>("users");
        }

        public List<User> GetAll() =>
            _users.Find(_ => true).ToList();

        public User GetUserByEmail(LoginModel model)
        {

            User user = _users.Find(user => user.Email == model.Email).FirstOrDefault();
            var hasher = new PasswordHasher<string>();
            var result = hasher.VerifyHashedPassword(null, user.Password, model.Password);
            if (result == PasswordVerificationResult.Success)
            {
                return user;
            }
            return null;
        }

        public void Add(User user)
        {
            
            var hasher = new PasswordHasher<string>();
            string hashedPassword = hasher.HashPassword(null, user.Password);
            user.Password = hashedPassword;
            _users.InsertOne(user);
        }
    }
}

