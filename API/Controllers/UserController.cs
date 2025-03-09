using API.Services;
using DataAccess.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("detail/{userId}")]
        public IActionResult GetUserById(int userId)
        {
            var user = _userService.GetUserById(userId);
            if (user == null) 
            {
                return NotFound("User not found.");
            }
            return Ok(user);
        }

        [HttpPost("change-password")]
        public IActionResult ChangePass([FromBody] ChangePassDto changePassDto)
        {
            try
            {
                _userService.ChangePass(changePassDto);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(
                    ex.Message
                );
            }
        }
    }
}
