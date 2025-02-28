using BCrypt.Net;
using DataAccess.DTOs.Auth;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
namespace API.Services
{
    public class AuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        public AuthService(IAuthRepository authRepository, IUserRepository userRepository, IConfiguration configuration)
        {
            _authRepository = authRepository;
            _userRepository = userRepository;
            _configuration = configuration;
        }
        public async Task<AuthenticateResponse?> Authenticate(AuthenticateRequest request)
        {
            if (request.Password == null || request.Email == null)
            {
                throw new ArgumentException("Email and Password cannot be null");
            }
            var user = await _userRepository.GetUserByEmail(request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return null; // Authentication failed
            }

            var AccessToken = GenerateAccessToken(user);
            var refreshToken = await GenerateRefreshToken();

            await _authRepository.SaveRefreshToken(HashToken(refreshToken), user);

            return new AuthenticateResponse(user, AccessToken, refreshToken);
        }

        public async Task<AuthenticateResponse> RefreshToken(string oldRefreshToken)
        {
            var oldTokenHash = HashToken(oldRefreshToken);
            var refreshToken = await _authRepository.FindRefreshToken(oldTokenHash);

            if (refreshToken == null)
            {
                throw new ArgumentException("Invalid refresh token");
            }
            if (refreshToken.Revoked == true)
            {
                throw new ArgumentException("Refresh token has been revoked");
            }
            if (refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                throw new ArgumentException("Refresh token has expired");
            }
            await RevokeRefreshToken(oldRefreshToken);
            var user = await _userRepository.GetUserById(refreshToken.UserId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var newAccessToken =  GenerateAccessToken(user);
            string newRefreshToken = await GenerateRefreshToken();

            await _authRepository.SaveRefreshToken(HashToken(newRefreshToken), user);

            return new AuthenticateResponse(user, newAccessToken, newRefreshToken);
        }
        private async Task<string> GenerateRefreshToken()
        {
            var refreshToken = await GetUniqueToken();
            return refreshToken;
        }
        private async Task<string> GetUniqueToken()
        {
            while (true)
            {
                var randomNum = RandomNumberGenerator.GetBytes(32);
                var token = Convert.ToBase64String(randomNum)
                    .Replace("+", "-")
                    .Replace("/", "_")
                    .TrimEnd('=');
                var tokenHash = HashToken(token);
                var existingToken = await _authRepository.FindRefreshToken(tokenHash);
                if (existingToken == null)
                {
                    return token;
                }
            }
        }
        private string HashToken(string token)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
                return Convert.ToBase64String(hashBytes);
            }

        }
        private async Task RevokeRefreshToken(string token)
        {
            var tokenHash = HashToken(token);
            var refreshToken = await _authRepository.FindRefreshToken(tokenHash);
            if (refreshToken != null)
            {
                refreshToken.Revoked = true;
                await _authRepository.UpdateRefreshTokenAsync(refreshToken);
            }

        }

        private string GenerateAccessToken(User user)
        {
            //The audience for the token (this should be the URL of the token issuer).          
            var audience = _configuration["Jwt:Audience"];
            //The issuer of the token (e.g., the URL of your authorization server).
            var issuer = _configuration["Jwt:Issuer"];
            var privateKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(privateKey))
            {
                throw new ArgumentException("JWT private key cannot be null or empty");
            }
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            if (user == null)
            {
                throw new ArgumentException("User cannot be null");
            }
            var claims = new List<Claim>
            {
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
             //The "jti" (JWT ID) claim provides a unique identifier for the JWT.
             new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
             new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),

              };
            if (user.Role?.RoleName == null)
            {
                throw new InvalidOperationException("User role is missing.");
            }
            claims.Add(new Claim(ClaimTypes.Role, user.Role.RoleName));

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
