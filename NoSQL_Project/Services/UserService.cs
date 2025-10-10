using Microsoft.AspNetCore.Identity;
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

        public async Task<User> AuthenticateUserAsync(LoginModel model)
        {
            return await _userRepository.AuthenticateUserAsync(model);
        }

        public async Task<User> FindByIdAsync(string id)
        {
            return await _userRepository.FindByIdAsync(id);
        }

		public async Task<User> FindUserByNameAsync(string firstName, string lastName)
        {
            return await _userRepository.FindUserByNameAsync(firstName, lastName);
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
            await UpdateUserAsync(user);
            return token;

        }

        public List<User> GetAll()
        {
            return _userRepository.GetAll().Result;
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetUserByEmailAsync(email);
        }

        public async Task UpdateUserAsync(User user)
        {
            _userRepository.UpdateUserAsync(user);
        }

        public User HashUserPassword(User user)
        {
            return _userRepository.HashUserPassword(user);
        }
    }
}
