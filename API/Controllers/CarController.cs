using API.Services;
using DataAccess.DTOs.CarDTO;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CarController : Controller
    {

        private CarService _carService;

        public CarController(CarService carService)
        {
            _carService = carService;
        }


        [HttpGet("owner")]
        public ActionResult<List<MyCarsDTO>> GetCarByOwner()
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

            var cars = _carService.GetCarByOwner(userId);
            if (cars == null || cars.Count == 0)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = "Không tìm thấy xe nào",
                    Status = 404
                });
            }

            return Ok(cars);
        }

        [Authorize("Driver")]
        [HttpGet("detail/{carId}")]
        public ActionResult<CarDetailDTO> GetCarDetailById(int carId)
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

            if (carId <= 0)
            {
                return BadRequest("Invalid car ID.");
            }

            var carDetail = _carService.GetCarDetailById(carId,userId);

            if (carDetail == null)
            {
                return NotFound("Car not found.");
            }

            return Ok(carDetail);
        }

        [Authorize("Driver")]
        [HttpGet("real-time-status/{carId}")]
        public ActionResult<ChargingStatusDTO> GetChargingStatus(int carId)
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
            var result = _carService.GetChargingStatusById(carId,userId);
            if (result == null)
            {
                return NotFound(new { message = "Not found" });
            }

            return Ok(result);
        }


        [Authorize("Driver")]
        [HttpGet("real-time-status-for-renter/{carId}")]
        public ActionResult<ChargingStatusDTO> GetChargingStatusForRenter(int carId)
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
            var result = _carService.GetChargingStatusByIdForRenter(carId, userId);
            if (result == null)
            {
                return NotFound(new { message = "Not found" });
            }

            return Ok(result);
        }

        [Authorize("Driver")]
        [HttpGet("charging-history")]
        public ActionResult<List<ChargingHistoryDTO>> GetChargingHistory(
            [FromQuery] int carId,
            [FromQuery] DateTime? start,
            [FromQuery] DateTime? end,
            [FromQuery] int? chargingStationId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
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
                var result = _carService.GetChargingHistory(carId, start, end, chargingStationId, userId, page, pageSize);
                if (result == null || result.Count == 0)
                {
                    return NotFound(new { message = "Not found" });
                }

                return Ok(result);


            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "Lỗi xử lý yêu cầu",
                    detail = ex.Message,
                    
                });
            }
        }


            [Authorize("Driver")]
        [HttpDelete("Delete/{carId}")]
        public IActionResult DeleteCar(int carId)
        {
            _carService.deleteCar(carId);
            

            return Ok(new { message = "Car deleted successfully" });
        }
        [Authorize("Driver")]
        [HttpGet("car-models")]
        public ActionResult<List<CarModel>> GetCarModels([FromQuery] string? search)
        {
            
                var carModels = _carService.GetCarModels(search);

                if (carModels == null || carModels.Count == 0)
                {
                    return NotFound(new { message = "No car models found." });
                }

                return Ok(carModels);
            
        }
        [Authorize("Driver")]
        [HttpPost("add-car")]
        public async Task<IActionResult> AddCar([FromForm] AddCarRequest request)
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
            if (request == null)
            {
                return BadRequest("Request body is null.");
            }

            if (request.CarModelId <= 0 || userId <= 0 || string.IsNullOrEmpty(request.LicensePlate) )
            {
                return BadRequest("Invalid input parameters.");
            }

            try
            {
                await _carService.addCar(request, userId);
                return Ok(new { message = "Car added successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Đã xảy ra lỗi khi thêm xe.",
                    error = ex.Message,
                    stack = ex.StackTrace, // Bỏ nếu ở production
                    inner = ex.InnerException?.Message
                });
            }
        }

        [HttpPut("edit-car")]
        public IActionResult EditCar([FromBody] EditCarRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is null.");
            }

            if (request.CarModelId <= 0 || request.CarId <= 0 || string.IsNullOrEmpty(request.LicensePlate) )
            {
                return BadRequest("Invalid input parameters.");
            }

            try
            {
                _carService.editCar(request.CarModelId, request.CarId, request.LicensePlate, request.CarName);
                return Ok(new { message = "Car updated successfully." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the car.", error = ex.Message });
            }
        }

        [HttpPost("send-rent-request")]
        public async Task<IActionResult> SendRentRequest([FromBody] RentRequestDto request)
        {
            try
            {
                await _carService.SendRentRequestForRent(request.UserId, request.CarId, request.StartDate, request.EndDate);
                return Ok(new { Message = "Rent request sent successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpGet("rent-requests")]
        public async Task<IActionResult> GetRentRequests()
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
            int driverId = int.Parse(userIdClaim.Trim());
            var rentRequests = await _carService.GetRentRequest(driverId);
            if (rentRequests == null || !rentRequests.Any())
            {
                return NotFound("No rent requests found for this driver.");
            }
            return Ok(rentRequests);
        }

        [HttpPut("confirm-rental")]
        public async Task<IActionResult> ConfirmRental([FromBody] ConfirmRentDto request)
        {
            bool updated = await _carService.ConfirmRentalAsync(request.UserId, request.CarId, request.Role);
            if (!updated)
                return NotFound(new { message = "Không tìm thấy thông tin thuê xe." });

            return Ok(new { message = "Xác nhận thuê xe thành công!" });
        }


        //[HttpPost("add-session")]
        //public async Task<IActionResult> AddSession([FromBody] ChargingSessionRequest request)
        //{
        //    try
        //    {
        //        // Gọi service để thêm phiên sạc
        //        var session = await _carService.AddChargingSession(
        //            request.CarId,
        //            request.PointId,
        //            request.TimeMoment,
        //            request.ChargingTime,
        //            request.Energy,
        //            request.Cost
        //        );

        //        // Trả về thông tin của phiên sạc vừa tạo
        //        return Ok(session);
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        return BadRequest(ex.Message); // Trả về thông báo lỗi nếu dữ liệu không hợp lệ
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex.Message}"); // Trả về lỗi server nếu có vấn đề
        //    }
        //}

        [HttpGet("{carId}/stats")]
        public IActionResult GetCarStats(int carId, int? year)
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
            var stats = _carService.GetCarStats(carId, year,userId);
            return Ok(stats);
        }

        [HttpGet("is-being-rented")]
        public async Task<IActionResult> IsCarBeingRentedAsync(int carId)
        {
            var response = await _carService.IsCarBeingRentedAsync(carId);
            return Ok(response);
        }

        [HttpGet("all-cars")]
        public IActionResult GetAllCars(
            [FromQuery] string? search,
            [FromQuery] string? type,
            [FromQuery] string? branch,
            [FromQuery] bool? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = _carService.GetAllCars(search, type, branch, status, page, pageSize);
            return Ok(result);
        }

        [HttpGet("filter-options")]
        public IActionResult GetFilterOptions()
        {
            var result = _carService.GetFilterOptions();
            return Ok(result);
        }

        [HttpPut("update-expired-rentals")]
        public async Task<IActionResult> UpdateExpiredRentals()
        {
            await _carService.UpdateExpiredRentalsAsync();
            return Ok();
        }
    }
}
