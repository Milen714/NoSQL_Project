using Microsoft.AspNetCore.Identity;
using NoSQL_Project.Models;
using NoSQL_Project.Models.Enums;
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
        public async Task<List<User>> GetActiveUsers(string searchString, UserType userTypeFilter, bool hasType)
        {
            return await _userRepository.GetActiveUsers(searchString, userTypeFilter, hasType);
        }

        public async Task Add(User user)
        {
            User hashedUser = HashUserPassword(user);
            await _userRepository.Add(hashedUser);
        }

        public async Task<User> AuthenticateUserAsync(LoginModel model)
        {
            User existingUser = await GetUserByEmailAsync(model.Email);
            if (existingUser == null)
                throw new Exception($"User with email {model.Email} not found");
            var hasher = new PasswordHasher<string>();
            var result = hasher.VerifyHashedPassword(null, existingUser.PasswordHash, model.Password);
            if (result == PasswordVerificationResult.Success)
            {
                return existingUser;
            }
            throw new Exception("Entered Password is Incorrect");

        }

        public async Task<User> FindByIdAsync(string id)
        {
            return await _userRepository.FindByIdAsync(id);
        }

		public async Task<User> FindUserByNameAsync(string firstName, string lastName)
        {
            return await _userRepository.FindUserByNameAsync(firstName, lastName);
		}

		private string GenerateSecureToken(int length = 32)
        {
            var bytes = RandomNumberGenerator.GetBytes(length);
            return Convert.ToBase64String(bytes);
        }
        public async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            var token = GenerateSecureToken();
            user.RessetToken = token;
            user.RessetTokenExpiry = DateTime.UtcNow.AddHours(1); 
            await UpdateUserAsync(user);
            return token;

        }
        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetUserByEmailAsync(email);
        }

        public async Task UpdateUserAsync(User user)
        {
           await _userRepository.UpdateUserAsync(user);
        }

        public User HashUserPassword(User user)
        {
            var hasher = new PasswordHasher<string>();
            string hashedPassword = hasher.HashPassword(null, user.PasswordHash);
            user.PasswordHash = hashedPassword;
            return user;
        }
    }
}
