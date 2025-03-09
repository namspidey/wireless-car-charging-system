using DataAccess.DTO;
using DataAccess.Interfaces;
using DataAccess.Models;

namespace API.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public UserDetailDto? GetUserById(int id) 
        {
            var user = _userRepository.GetUserById(id);
            return user;
        }

        public void ChangePass(ChangePassDto changePassDto)
        {
            _userRepository.ChangePass(changePassDto);
        }
    }
}
