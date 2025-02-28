using Azure.Core;
using DataAccess.DTO.Auth;
using DataAccess.Interfaces;

namespace API.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task RegisterAsync(RegisterRequest request)
        {
            if (request == null)
            {
                throw new ArgumentException("Request cannot be null");
            }
            if (string.IsNullOrEmpty(request.Email))
            {
                throw new ArgumentException("Email cannot be null or empty");
            }

            var existingUser = await _userRepository.GetUserByEmail(request.Email);
            if (existingUser != null)
            {
                throw new ArgumentException("Email already exists");
            }
            request.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.PasswordHash);

            await _userRepository.SaveUser(request);
        }

    }
}
