using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DataAccess.DTO.Auth
{
    public class AuthenticateResponse
    {
        public int Id { get; set; }
        public string? Fullname { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public string? AccessToken { get; set; }
        [JsonIgnore]
        public string RefreshToken { get; set; }
        public AuthenticateResponse(User user, string jwtToken, string refreshToken)
        {
            Id = user.UserId;
            Fullname = user.Fullname;
            Email = user.Email;
            Role = user.Role?.RoleName ?? "User"; // Giá trị mặc định
            AccessToken = jwtToken;
            RefreshToken = refreshToken;
        }
    }
}
