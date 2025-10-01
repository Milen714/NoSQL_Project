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
            _users = db.GetCollection<User>("USERS");
        }

        public List<User> GetAll()
        {
            try
            {
                var users = _users.Find(_ => true).ToList();
                return users;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception("Could not retrieve users.");
            }
        }

        public User GetUserByEmail(string email)
        {
            User user = _users.Find(user => user.EmailAddress == email).FirstOrDefault();
            return user;
        }
        public User AuthenticateUser(LoginModel model)
        {
            User existingUser = GetUserByEmail(model.Email);
            if (existingUser == null)
                return null;
            var hasher = new PasswordHasher<string>();
            var result = hasher.VerifyHashedPassword(null, existingUser.PasswordHash, model.Password);
            if (result == PasswordVerificationResult.Success)
            {
                return existingUser;
            }
            return null;

        }
        public User HashUserPassword(User user)
        {
            var hasher = new PasswordHasher<string>();
            string hashedPassword = hasher.HashPassword(null, user.PasswordHash);
            user.PasswordHash = hashedPassword;
            return user;
        }
        public void Add(User user)
        {
            try
            {
                var hashedUser = HashUserPassword(user);
                _users.InsertOne(hashedUser);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("DuplicateKey"))
                {
                    throw new Exception("A user with this email already exists.");
                }
            }
        }

        public User FindById(string id)
        {
            User user = _users.Find(user => user.Id == id).FirstOrDefault();
            return user;
        }
        public async Task UpdateUser(User user)
        {
            FilterDefinition<User> filter =
                    Builders<User>.Filter.Eq(u => u.Id, user.Id);
            await _users.ReplaceOneAsync(filter, user);
        }
    }
}

