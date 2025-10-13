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

        public async Task<List<User>> GetAll()
        {
            try
            {
                
                return await _users.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new Exception("Could not retrieve users.");
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _users.Find(user => user.EmailAddress == email).FirstOrDefaultAsync();
        }
        public async Task<User> AuthenticateUserAsync(LoginModel model)
        {
            User existingUser = await GetUserByEmailAsync(model.Email);
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
        public async Task Add(User user)
        {
            try
            {
                var hashedUser = HashUserPassword(user);
                await _users.InsertOneAsync(hashedUser);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                if (ex.Message.Contains("DuplicateKey"))
                {
                    throw new Exception("A user with this email already exists.");
                }
            }
        }

        public async Task<User> FindByIdAsync(string id)
        {
            User user = await _users.Find(user => user.Id == id).FirstOrDefaultAsync();
            return user;
        }
        public async Task UpdateUserAsync(User user)
        {
            FilterDefinition<User> filter =
                    Builders<User>.Filter.Eq(u => u.Id, user.Id);
            await _users.ReplaceOneAsync(filter, user);
        }

		public async Task <User> FindUserByNameAsync(string firstName, string lastName) 
        {        
            User user = await _users.Find(user => user.FirstName == firstName && user.LastName == lastName).FirstOrDefaultAsync();
			return user;
		}

		//public void DeleteEmployee(string id)
		//{
		//    FilterDefinition<User> filter = Builders<Users>.Filter.Eq(u => u.Id, user.id);
		//    _users.DeleteOne(filter);
		//}
	}
}

