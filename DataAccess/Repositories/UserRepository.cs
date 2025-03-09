using Azure.Core;
using DataAccess.DTO;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DataAccess.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly WccsContext _context;

        public UserRepository(WccsContext context)
        {
            _context = context;
        }

        public void ChangePass(ChangePassDto passDTO)
        {
            var user =  _context.Users.Find(passDTO.UserId);
            if (user == null)
                throw new KeyNotFoundException("User not found");
            if (string.IsNullOrWhiteSpace(passDTO.Password))
            {
                throw new ArgumentException("Email cannot be blank or contain only spaces", nameof(passDTO.Password));
            }
            if (string.IsNullOrWhiteSpace(passDTO.NewPassword))
            {
                throw new ArgumentException("Email cannot be blank or contain only spaces", nameof(passDTO.NewPassword));
            }
            if (string.IsNullOrWhiteSpace(passDTO.ConfirmNewPassword))
            {
                throw new ArgumentException("Email cannot be blank or contain only spaces", nameof(passDTO.ConfirmNewPassword));
            }
            if (!BCrypt.Net.BCrypt.Verify(passDTO.Password, user.PasswordHash)) 
            {
                throw new ArgumentException("Password is wrong", nameof(passDTO.Password));
            }
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passDTO.NewPassword);
            _context.Update(user);
            _context.SaveChanges();
        }

        public UserDetailDto? GetUserById(int id)
        {
            var user = _context.Users.Include(u => u.Role)
                .AsNoTracking().Where(u => u.UserId == id).Select(u => new UserDetailDto
                {
                    Id = u.UserId,
                    FullName = u.Fullname,
                    Email = u.Email,
                    PhoneNumber = u.PhoneNumber,
                    Dob = u.Dob,
                    Gender = u.Gender,
                    Address = u.Address,
                    Status = u.Status,
                    RoleName = u.Role.RoleName,
                    CreateAt = u.CreateAt,
                    UpdateAt = u.UpdateAt,
                })
                .SingleOrDefault();
            return user;
        }
    }
}
