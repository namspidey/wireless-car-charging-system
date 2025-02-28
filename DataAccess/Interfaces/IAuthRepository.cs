using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IAuthRepository
    {
         Task SaveRefreshToken(string token, User user);
        Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
        Task<RefreshToken?> FindRefreshToken(string token);   
    }
}
