using API.Services;
using DataAccess.DTOs;
using DataAccess.DTOs.Auth;
using DataAccess.DTOs.CarDTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;

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

        [EnableRateLimiting("Register")]
        [HttpPost("register")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> RegisterAsync([FromForm] RegisterRequest request)
        {
            try
            {
                await _userService.RegisterAsync(request);
                return Ok(new { result = "Đăng kí thành công" });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new
                {
                    Title = "Conflict",
                    Error = "Trùng lặp",
                    Detail = ex.Message
                });
            }
            catch (ArgumentException e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = e.Message,
                    Status = 500
                });
            }          
            catch (Exception e)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = e.Message,
                    Status = 400
                });
            }
           
        }
        [EnableRateLimiting("Login")]
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] AuthenticateRequest request)
        {
            try
            {

                var response = await _authService.Authenticate(request);
                if (response == null)
                {
                    return Unauthorized(new ProblemDetails
                    {
                        Title = "Unauthorized",
                        Detail = "Email hoặc password không đúng",
                        Status = 401
                    });
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
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = e.Message,
                    Status = 400
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = e.Message,
                    Status = 500
                });
            }
        }
        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            try
            {

                if (Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
                {
                    await _authService.Logout(refreshToken);
                }
                Response.Cookies.Delete("refreshToken", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Path = "/"
                });

                return Ok(new { Result = "Đăng xuất thành công" });
            }
            catch (ArgumentException e)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = e.Message,
                    Status = 400
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = e.Message,
                    Status = 500
                });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken1()
        {
            try
            {
                if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
                {
                    return Unauthorized(new ProblemDetails
                    {
                        Title = "Unauthorized",
                        Detail = "Không tìm thấy refresh token",
                        Status = 401
                    });
                }

                var response = await _authService.RefreshToken(refreshToken);
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
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = e.Message,
                    Status = 400
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = e.Message,
                    Status = 500
                });
            }
        }

        [HttpGet("profile/{userId}")]
        public async Task<IActionResult> GetProfileByUserId(int userId)
        {
            try
            {
                var profile = await _userService.GetProfileByUserId(userId);
                return Ok(profile);
            }
            catch (ArgumentException e)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = e.Message,
                    Status = 400
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = e.Message,
                    Status = 500
                });
            }
        }

        [HttpPut("profile/{userId}")]
        public async Task<IActionResult> UpdateUserProfile(int userId, [FromBody] RequestProfile request)
        {
            try
            {
                await _userService.UpdateUserProfileAsync(userId, request);
                return Ok(new { Message = "Profile updated successfully" });
            }
            catch (ArgumentException e)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = e.Message,
                    Status = 400
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = e.Message,
                    Status = 500
                });
            }
        }


        
    }
}
