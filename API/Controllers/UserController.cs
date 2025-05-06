using API.Services;
using DataAccess.DTOs.CarDTO;
using DataAccess.DTOs.UserDTO;
using DataAccess.DTOs.Auth;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        public UserController(UserService userService)
        {
            _userService = userService;
        }
        [AllowAnonymous]
        [HttpPost("email")]
        public async Task<IActionResult> GetUserByEmail([FromBody] EmailRequest emailRequest)
        {
            try
            {
                var user = await _userService.GetUserByEmail(emailRequest.Email);
                return Ok(user);
            }
            catch (ArgumentException e)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = e.Message,
                    Status = 404
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau." + e.Message,
                    Status = 500
                });
            }
        }

        [Authorize("DriverOrOperator")]
        [HttpGet("driver-licenses/{licenseCode}")]
        public async Task<IActionResult> GetDriverLicense(string licenseCode)
        {
            try
            {
                var driverLicense = await _userService.GetLicenseByCode(licenseCode);
                return Ok(driverLicense);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = ex.Message,
                    Status = 404
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau."+ex.Message,
                    Status = 500
                });
            }
        }

        [Authorize("Driver")]
        [HttpPost("driver-licenses")]
        public async Task<IActionResult> AddDriverLicense([FromForm] DriverLicenseRequest request)
        {
                       var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Unauthorized",
                    Detail = "Bạn cần đăng nhập để thực hiện thao tác này",
                    Status = 401
                });
            }
            int userId = int.Parse(userIdClaim.Trim());
            
            try
            {
                await _userService.AddDriverLicenseAsync(userId, request);
                return Ok(new {result="Thêm bang lái thanh cong"});
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ProblemDetails {
                    Title = "Bad Request",
                    Detail = ex.Message,
                    Status = 400
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ProblemDetails{
                    Title = "Internal Server Error",
                    Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau." + ex.Message,
                    Status = 500
                });
            }
        }
        // PUT: api/driver-licenses/{licenseCode}
        [Authorize("Driver")]
        [HttpPut("driver-licenses/{licenseCode}")]
        public async Task<IActionResult> UpdateDriverLicense(string licenseCode,[FromForm] DriverLicenseRequest request)
        {
            
            try
            {
                await _userService.UpdateDriverLiscense(licenseCode, request);
                return Ok(new { result ="Cập nhật thành công" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Detail = ex.Message,
                    Status = 400
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ProblemDetails {
                    Title = "Internal Server Error",
                    Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau." + ex.Message,
                    Status = 500
                });
            }
        }

        // DELETE: api/driver-licenses/{licenseCode}
        [Authorize("DriverOrOperator")]
        [HttpDelete("driver-licenses/{licenseCode}")]
        public async Task<IActionResult> DeleteDriverLicense(string licenseCode)
        {
            try
            {
                await _userService.DeleteDriverLicenseAsync(licenseCode);
                return Ok(new { result = "Xoá bằng lái thành công" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = ex.Message,
                    Status = 404
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau." + ex.Message,
                    Status = 500
                });
            }
        }
        [Authorize("DriverOrOperator")]
        [HttpPut("driver-licenses/{licenseCode}/activate")]
        public async Task<IActionResult> ActiveDriverLicense(string licenseCode)
        {
            try
            {
                await _userService.ActiveDriverLicenseAsync(licenseCode);
                return Ok(new { result = "Kích hoạt bằng lái thành công" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = ex.Message,
                    Status = 404
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau." + ex.Message,
                    Status = 500
                });
            }
        } 
        [Authorize("DriverOrOperator")]
        [HttpPut("driver-licenses/{licenseCode}/block")]
        public async Task<IActionResult> BlockDriverLicense(string licenseCode)
        {
            try
            {
                await _userService.BlockedDriverLicenseAsync(licenseCode);
                return Ok(new { result = "Thành công khoá bằng lái" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = ex.Message,
                    Status = 404
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau." + ex.Message,
                    Status = 500
                });
            }
        }
        [Authorize("Driver")]
        [HttpGet("driver-licenses/users")]
        public async Task<IActionResult> GetDriverLicenseByUser()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Unauthorized",
                    Detail = "Bạn cần đăng nhập để thực hiện thao tác này",
                    Status = 401
                });
            }
            int userId = int.Parse(userIdClaim.Trim());
            try
            {
                var driverLicense = await _userService.GetActiveDriverLicensesAsync(userId);
                return Ok(driverLicense);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Bad Request",
                    Detail = ex.Message,
                    Status = 400
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau." + ex.Message,
                    Status = 500
                });
            }
        }

        [Authorize("Operator")]
        [HttpGet]
        public IActionResult GetUsers(
            [FromQuery] string? searchQuery,
            [FromQuery] string? status,
            [FromQuery] int? roleId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5)
        {
            var result = _userService.GetUsers(searchQuery, status, roleId, pageNumber, pageSize);
            return Ok(result);
        }

        [Authorize("Operator")]
        [HttpPut("change-status/{userId}")]
        public async Task<IActionResult> ChangeUserStatus(int userId, [FromBody] string newStatus)
        {
            await _userService.ChangeUserStatusAsync(userId, newStatus);
            return Ok(new { message = "Cập nhật trạng thái người dùng thành công." });
        }        

        [AllowAnonymous]
        [HttpGet("licenses")]
        public async Task<ActionResult<PagedResultD<DriverLicenseDTO>>> GetPagedLicenses([FromQuery] DriverLicenseFilter filter, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _userService.GetLicenseList( pageNumber, pageSize, filter);
            return Ok(result);
        }

        //[Authorize("Operator")]
        [AllowAnonymous]
        [HttpPut("driver-licenses/{licenseCode}/update-operator")]
        public async Task<IActionResult> UpdateDriverLicenseOperator(string licenseCode, [FromBody] UpdateDriverLicenseOperatorRequest request)
        {
            try
            {
                await _userService.UpdateDriverlicenseOperator(licenseCode, request.NewCode, request.Fullname, request.Level);
                return Ok(new { result = "Cập nhật bằng lái thành công" });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = ex.Message,
                    Status = 404
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau." + ex.Message,
                    Status = 500
                });
            }
        }


        [HttpGet("profile")]
        public async Task<IActionResult> GetProfileByUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Unauthorized",
                    Detail = "Bạn cần đăng nhập để thực hiện thao tác này",
                    Status = 401
                });
            }
            int userId = int.Parse(userIdClaim.Trim());
            try
            {
                var profile = await _userService.GetProfileByUserId(userId);
                return Ok(profile);
            }
            catch (ArgumentException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                return StatusCode(500, new { Message = "Internal server error", Error = e.Message });
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateUserProfile( [FromBody] RequestProfile request)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Unauthorized",
                    Detail = "Bạn cần đăng nhập để thực hiện thao tác này",
                    Status = 401
                });
            }
            int userId = int.Parse(userIdClaim.Trim());
            try
            {
                await _userService.UpdateUserProfileAsync(userId, request);
                return Ok(new { Message = "Profile updated successfully" });
            }
            catch (ArgumentException e)
            {
                return BadRequest(new { Message = e.Message });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { Message = "Internal server error", Error = e.Message });
            }
        }


        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePassDTO passDTO)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Unauthorized",
                    Detail = "Bạn cần đăng nhập để thực hiện thao tác này",
                    Status = 401
                });
            }
            int userId = int.Parse(userIdClaim.Trim());

            try
            {
                await _userService.ChangePasswordAsync(userId,passDTO);
                return Ok(new { Message = "Đổi mật khẩu thành công" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Message = "Lỗi hệ thống" });
            }
        }

        [HttpGet("get-users-by-email-phone")]
        public async Task<IActionResult> GetUsersByEmailPhone([FromQuery] string search)
        {
            try
            {
                var users = await _userService.GetUsersByEmailOrPhoneAsync(search);
                if (users == null || users.Count == 0)
                {
                    return NotFound("Không tìm thấy người dùng nào.");
                }
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message); // hoặc StatusCode(500) nếu cần
            }
        }

        [HttpGet("having-driver-license-yet")]
        public async Task<IActionResult> HavingDriverLicenseYet()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return Unauthorized(new ProblemDetails
                {
                    Title = "Unauthorized",
                    Detail = "Bạn cần đăng nhập để thực hiện thao tác này",
                    Status = 401
                });
            }
            int userId = int.Parse(userIdClaim.Trim());
            var result =await _userService.HavingDriverLicenseYet(userId);
            return Ok(result);
        }
    }
}
