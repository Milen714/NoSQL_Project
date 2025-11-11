using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;
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

       
        public async Task<List<User>> GetActiveUsers(string searchString, UserType userTypeFilter, bool hasType)
        {
            FilterDefinition<User> emailFilter = FilterDefinition<User>.Empty;
            FilterDefinition<User> typeFilter = FilterDefinition<User>.Empty;
            try
            {
                if (!string.IsNullOrWhiteSpace(searchString))
                {
                    emailFilter = Builders<User>.Filter.Regex(u => u.EmailAddress, new MongoDB.Bson.BsonRegularExpression(searchString, "i"));
                }
                if (hasType)
                {
                    typeFilter = Builders<User>.Filter.Eq(u => u.UserType, userTypeFilter);
                }
                var combinedFilter = Builders<User>.Filter.And(emailFilter, typeFilter);
                var result = await _users.Find(combinedFilter).ToListAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _users.Find(user => user.EmailAddress == email).FirstOrDefaultAsync();
        }
        
        public async Task Add(User user)
        {
            try
            {
                await _users.InsertOneAsync(user);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("phone_number"))
                {
                    throw new Exception("A user with this Phone number already exists.");
                }
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

