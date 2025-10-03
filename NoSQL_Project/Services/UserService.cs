using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using NoSQL_Project.Models;
using NoSQL_Project.Repositories.Interfaces;
using NoSQL_Project.Services.Interfaces;
using System.Security.Cryptography;

namespace NoSQL_Project.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<User> _userManager;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public void Add(User user)
        {
            _userRepository.Add(user);
        }

        public User AuthenticateUser(LoginModel model)
        {
            return _userRepository.AuthenticateUser(model);
        }

        public User FindById(string id)
        {
            return _userRepository.FindById(id);
        }

        public string GenerateSecureToken(int length = 32)
        {
            var bytes = RandomNumberGenerator.GetBytes(length);
            return Convert.ToBase64String(bytes);
        }
        public async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            var token = GenerateSecureToken();
            user.RessetToken = token;
            user.RessetTokenExpiry = DateTime.UtcNow.AddHours(1); // Token valid for 1 hour
            await UpdateUser(user);
            return token;

        }

        public IQueryable<User> GetAll()
        {
            return _userRepository.GetAll(); 
        }

        public User GetUserByEmail(string email)
        {
            return _userRepository.GetUserByEmail(email);
        }

        public async Task UpdateUser(User user)
        {
            _userRepository.UpdateUser(user);
        }

        public User HashUserPassword(User user)
        {
            return _userRepository.HashUserPassword(user);
        }

        public async Task<ReporterSnapshot> GetReporterSnapshotAsync(string userId)
		{
			var user = FindById(userId);

			if (user == null) 
                return null;

			return new ReporterSnapshot
			{
			    UserId = ObjectId.Parse(user.Id),
			    FirstName = user.FirstName,
				LastName = user.LastName,
                EmailAddress = user.EmailAddress
			};
		}
	}
}
