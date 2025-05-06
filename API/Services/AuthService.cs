using BCrypt.Net;
using DataAccess.DTOs.Auth;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
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

            if (string.IsNullOrWhiteSpace(request.CaptchaToken))
            {
                throw new ArgumentException("Captcha không được để trống", nameof(request.CaptchaToken));
            }

            var captchaValid = await VerifyCaptchaAsync(request.CaptchaToken);
            if (!captchaValid)
            {
                throw new ArgumentException("Captcha không hợp lệ", nameof(request.CaptchaToken));
            }

            if (request.Password.IsNullOrEmpty())
            {
                throw new ArgumentException("Password không thể trống hoặc khoảng trắng",nameof(request.Password));
            }
            var user = await _userRepository.GetUserByEmail(request.Email);
            var refreshtokenNeedRevole = user!.RefreshTokens
                .OrderByDescending(rt => rt.CreatedAt) 
                .FirstOrDefault(); 
            if (refreshtokenNeedRevole != null)
            {
                if (refreshtokenNeedRevole.Revoked!= true)
                {
                    await RevokeRefreshTokenNoHash(refreshtokenNeedRevole.Token!);
                }
            }
            if (request.Email.Length > 225) 
            {
                throw new ArgumentException("Email không được vượt quá 225 ký tự.", nameof(request.Email));
            }
            if (request.Password.Length > 100) 
            {
                throw new ArgumentException("Mật khẩu không được vượt quá 100 ký tự.", nameof(request.Password));
            }
            if (user == null)
            {
                throw new ArgumentException("Tài khoản không tồn tại");
            }
            if ( !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new ArgumentException("Mật khẩu sai"); // Authentication failed
            }
            if (user.Status == "PENDING")
            {
                throw new ArgumentException("Tài khoản của bạn đang trong hàng chờ, vui lòng đợi duyệt.");
            }
            if (user.Status == "REJECTED")
            {
                throw new ArgumentException("Tài khoản của bạn đã bị từ chối. Vui lòng liên hệ hỗ trợ để biết thêm chi tiết.");
            }
            if (user.Status == "OTPprocess")
            {
                throw new ArgumentException("Tài khoản của bạn chưa hoàn tất đăng kí. Vui lòng đăng kí lại sau 1 ngày");
            }
            if (user.Status != "ACTIVE")
            {
                throw new ArgumentException("Tài khoản của bạn không đủ điều kiện để thực hiện thao tác này.");
            }

            var AccessToken = GenerateAccessToken(user);
            var refreshToken = await GenerateRefreshToken();

            await _authRepository.SaveRefreshToken(HashToken(refreshToken), user);

            return new AuthenticateResponse(user, AccessToken, refreshToken);
        }

        public async Task<AuthenticateResponse> RefreshToken(string oldRefreshToken)
        {
            if (string.IsNullOrEmpty(oldRefreshToken))
            {
                throw new ArgumentException("Refresh token không thể trống hoặc khoảng trắng", nameof(oldRefreshToken));
            }
            var oldTokenHash = HashToken(oldRefreshToken);
            var refreshToken = await _authRepository.FindRefreshToken(oldTokenHash);

            if (refreshToken == null)
            {
                throw new ArgumentException("Refresh token không hợp lệ");
            }
            if (refreshToken.Revoked == true)
            {
                throw new ArgumentException("Refresh token đã bị thu hồi");
            }
            if (refreshToken.ExpiresAt < DateTime.UtcNow)
            {
                throw new ArgumentException("Refresh token đã hết hạn");
            }
            await RevokeRefreshToken(oldRefreshToken);
            var user = await _userRepository.GetUserById(refreshToken.UserId);
            if (user == null)
            {
                throw new ArgumentException("Người dùng không tồn tại");
            }

            var newAccessToken = GenerateAccessToken(user);
            string newRefreshToken = await GenerateRefreshToken();

            await _authRepository.SaveRefreshToken(HashToken(newRefreshToken), user);

            return new AuthenticateResponse(user, newAccessToken, newRefreshToken);
        }
        public async Task Logout(string refreshToken)
        {
            await RevokeRefreshToken(refreshToken);
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
        private async Task RevokeRefreshTokenNoHash(string token)
        {
            var refreshToken = await _authRepository.FindRefreshToken(token);
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
            var privateKey = Environment.GetEnvironmentVariable("JWT_SECRET");
            if (string.IsNullOrEmpty(privateKey))
            {
                throw new ArgumentException("Khoá bí mật không thể trống");
            }
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            if (user == null)
            {
                throw new ArgumentException("Người dùng không được để trống");
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
                throw new InvalidOperationException("Role người dùng đang thiếu");
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
        public async Task<bool> VerifyCaptchaAsync(string captchaResponse)
        {
            if (string.IsNullOrEmpty(captchaResponse))
                return false;

            var _secretKey = Environment.GetEnvironmentVariable("RECAPTCHA_SECRET_KEY");
            if (string.IsNullOrEmpty(_secretKey))
            {
             throw new ArgumentException("Khoá bí mật không thể trống");
            }
            using (var client = new HttpClient())
            {
                var parameters = new Dictionary<string, string>
                {
                    { "secret", _secretKey },
                    { "response", captchaResponse }
                };

                var content = new FormUrlEncodedContent(parameters);
                var response = await client.PostAsync("https://www.google.com/recaptcha/api/siteverify", content);
                var responseString = await response.Content.ReadAsStringAsync();

                var captchaResult = JsonConvert.DeserializeObject<CaptchaResponse>(responseString);

                return captchaResult!.success;
            }
        }


    }
}
