using API.Services;
using DataAccess.DTOs.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class OtpController : ControllerBase
    {
        private readonly OtpServices _otpServices;
        private readonly EmailService _emailService;
        private readonly UserService _userService;
        private readonly string _otpTemplate;
        private readonly string _activationTemplate;
        private readonly string _frontendUrl;


        public OtpController(OtpServices otpServices, EmailService emailService, UserService userService)
        {
            _otpServices = otpServices;
            _emailService = emailService;
            _otpTemplate = System.IO.File.ReadAllText("Template/OTPEmailTemplate.html");
            _activationTemplate = System.IO.File.ReadAllText("Template/ActivationEmailTemplate.html");
            _frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "https://wireless-charging-system.azurewebsites.net";
            _userService = userService;
        }
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateOtp([FromBody] OtpRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ModelState.Values.FirstOrDefault()?.Errors.FirstOrDefault()?.ErrorMessage,
                    Status = 400
                });
            }

            try
            {
                var otp = await _otpServices.GenerateOtpAsync(request);
                var emailBody = _otpTemplate.Replace("{{OTP}}", otp);
                await _emailService.SendEmailAsync(request.Email, "OTP Verification", emailBody);
                return Ok(new { Message = "OTP created success" });
            }
            catch (ArgumentException ex) 
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ex.Message,
                    Status = 400
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.",
                    Status = 500,
                });
            }

        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ModelState.Values.FirstOrDefault()?.Errors.FirstOrDefault()?.ErrorMessage,
                    Status = 400
                });
            }

            try
            {
                bool isValid = await _otpServices.VerifyOtpAsync(request.Email, request.OtpCode);
                if (isValid)
                {
                    return Ok(new { Message = "OTP verified successfully." });
                }
                return BadRequest(new ProblemDetails
                {
                    Title = "Mã OTP không hợp lệ",
                    Detail = "Mã OTP không đúng hoặc đã hết hạn",
                    Status = 400
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ex.Message,
                    Status = 400
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.",
                    Status = 500,
                });
            }
        }
        [HttpPost("pending-register")]
        public async Task<IActionResult> PendingRegisterUserUseOTP([FromBody] VerifyOtpRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ModelState.Values.FirstOrDefault()?.Errors.FirstOrDefault()?.ErrorMessage,
                    Status = 400
                });
            }

            try
            {
                await _userService.PendingRegisterUserViaOtpAsync(request.Email, request.OtpCode);
                return Ok(new
                {
                    Message = "Xác thực OTP thành công. Tài khoản đang chờ kích hoạt."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ex.Message,
                    Status = 400
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.",
                    Status = 500,
                });
            }
        } 
        [HttpPost("verify_and_create_reset_password_token")]
        public async Task<IActionResult> VerifyAndGenerateTokenReset([FromBody] VerifyOtpRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ModelState.Values.FirstOrDefault()?.Errors.FirstOrDefault()?.ErrorMessage,
                    Status = 400
                });
            }

            try
            {
                var token = await _userService.VerifyAndGenResetPasswordToken(request.Email, request.OtpCode);
                return Ok(new { Message = "OTP created success", Token = token });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ex.Message,
                    Status = 400
                });
            }
            catch (Exception e)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.",
                    Status = 500,
                });
            }
        }

        //[HttpPost("reset-token")]
        //public async Task<IActionResult> GenerateTokenReset([FromBody] string Email)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(new ProblemDetails
        //        {
        //            Title = "Invalid Request",
        //            Detail = ModelState.Values.FirstOrDefault()?.Errors.FirstOrDefault()?.ErrorMessage,
        //            Status = 400
        //        });
        //    }
        //    try
        //    {
        //        var token = await _otpServices.genResetPasswordToken(Email);
        //        return Ok(new { Message = "OTP created success", Token = token });
        //    }
        //    catch
        //    {
        //        return StatusCode(500, new ProblemDetails
        //        {
        //            Title = "Internal Server Error",
        //            Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.",
        //            Status = 500,
        //        });
        //    }
        //}
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ModelState.Values.FirstOrDefault()?.Errors.FirstOrDefault()?.ErrorMessage,
                    Status = 400
                });
            }
            try
            {
                var isValid = await _userService.ResetPassword(request);
                if (isValid)
                {
                    return Ok(new { Message = "Password reset successfully." });
                }
                return BadRequest(new ProblemDetails
                {
                    Title = "Token không hợp lệ",
                    Detail = "Token không đúng hoặc đã hết hạn",
                    Status = 400
                });
            }
            catch
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.",
                    Status = 500,
                });
            }
        }
        [HttpPost("activation-token")]
        public async Task<IActionResult> GenerateActivationToken([FromBody] OtpRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ModelState.Values.FirstOrDefault()?.Errors.FirstOrDefault()?.ErrorMessage,
                    Status = 400
                });
            }

            var user = await _userService.GetUserByEmail(request.Email);
            if (user == null)
            {
                return NotFound(new
                {
                    Title = "Người dùng không tồn tại",
                    Detail = "Không tìm thấy người dùng.",
                    Status = 404
                });

            }
            try
            {
                var token = await _otpServices.GenerateAccountActivationTokenAsync(request.Email);
                var activationLink = $"{_frontendUrl}/Wireless-charging/Auth/activate?token={token}&email={request.Email}";
                var body = _activationTemplate
               .Replace("{{ActivationLink}}", activationLink)
               .Replace("{{UserName}}", user.Fullname);
                var emailBody = _activationTemplate.Replace("{{ActivationLink}}", token);
                await _emailService.SendEmailAsync(request.Email, "Kích hoạt tài khoản", body);
                return Ok(new { Message = "Activation email sent." });
            }
            catch
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.",
                    Status = 500,
                });

            }
        }
        [HttpPost("activate")]
        public async Task<IActionResult> ConfirmActivation([FromBody] VerifyActiveAccountRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = ModelState.Values.FirstOrDefault()?.Errors.FirstOrDefault()?.ErrorMessage,
                    Status = 400
                });
            var valid = await _otpServices.VerifyActivationTokenAsync(request.OtpCode, request.Email);
            if (!valid)
            {
                return BadRequest(
                    new ProblemDetails
                    {
                        Title = "Invalid Token",
                        Detail = "Token không đúng hoặc đã hết hạn",
                        Status = 400
                    });
            }
            try
            {
                await _userService.ActiveAccount(request.Email);

                return Ok(new { Message = "Kích hoạt tài khoản thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.",
                    Status = 500,
                });

            }
        }
    }
}
