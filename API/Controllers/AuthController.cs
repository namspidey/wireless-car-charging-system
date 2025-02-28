using API.Services;
using DataAccess.DTOs.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly UserService _userService;
        public AuthController(AuthService authService, UserService userService)
        {
            _authService = authService;
            _userService = userService;
        }
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterRequest request)
        {
            try
            {
                await _userService.RegisterAsync(request);
                return Ok("Register Success");
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] AuthenticateRequest request)
        {
            try
            {
                var response = await _authService.Authenticate(request);
                if (response == null)
                {
                    return Unauthorized("Invalid email or password");
                }
                Response.Cookies.Append("refreshToken", response.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });

                return Ok(response);
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken1(string token)
        {
            try
            {
                var response = await _authService.RefreshToken(token);
                Response.Cookies.Append("refreshToken", response.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddDays(7)
                });
                return Ok(response);
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
