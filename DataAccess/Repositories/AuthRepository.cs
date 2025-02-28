using BCrypt.Net;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace DataAccess.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly WccsContext _context;
        public AuthRepository(WccsContext context)
        {
            _context = context;
        }

        public async Task<RefreshToken?> FindRefreshToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("Token cannot be blank or contain only spaces", nameof(token));
            }
           
                return await _context.RefreshTokens
                    .SingleOrDefaultAsync(rt=>rt.Token == token);
        }

        public async Task SaveRefreshToken(string token, User user)
        {
            RefreshToken refreshToken = new RefreshToken();
            refreshToken.Token = token;
            refreshToken.UserId = user.UserId;
            refreshToken.CreatedAt = DateTime.UtcNow;
            refreshToken.ExpiresAt = DateTime.UtcNow.AddDays(7);
             _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
        }
    }
}
