using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs.Auth
{
    public class RegisterRequest
    {
        public string? Fullname { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? Dob { get; set; }
        public bool? Gender { get; set; }
        public string? PasswordHash { get; set; }
    }
}
