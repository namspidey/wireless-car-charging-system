using DataAccess.DTO.Auth;
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
    public class UserRepository : IUserRepository
    {
        private readonly WccsContext _context;
        public UserRepository(WccsContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            if (String.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be blank or contain only spaces", nameof(email));
            }

            return await _context.Users.Include(u => u.Role)
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserById(int id)
        {
            return await _context.Users.Include(u => u.Role)
                .AsNoTracking()
                .SingleOrDefaultAsync(u => u.UserId == id);
        }

        public async Task SaveUser(RegisterRequest user)
        {
            User newUser = new User();
            if (user == null)
            {
                throw new ArgumentException("User cannot be null", nameof(user));
            }
            if (user.Email == null)
            {
                throw new ArgumentException("Email cannot be null", nameof(user.Email));
            }
            if (user.Fullname == null)
            {
                throw new ArgumentException("Fullname cannot be null", nameof(user.Fullname));
            }
            if (user.PhoneNumber == null)
            {
                throw new ArgumentException("PhoneNumber cannot be null", nameof(user.PhoneNumber));
            }
            newUser.Email = user.Email;
            newUser.Fullname = user.Fullname;
            newUser.PhoneNumber = user.PhoneNumber;
            newUser.Dob = user.Dob;
            newUser.RoleId = 1;
            newUser.Gender = user.Gender;
            newUser.PasswordHash = user.PasswordHash;
            newUser.Status = "Active";
            newUser.CreateAt = DateTime.Now;
            newUser.UpdateAt = DateTime.Now;
            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();
        }
    }
}

