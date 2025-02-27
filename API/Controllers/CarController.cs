using API.Services;
using DataAccess.DTO;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
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
        public ActionResult<List<MyCarsDTO>> GetCarByOwner([FromQuery] int userId)
        {
            if (userId <= 0)
            {
                return BadRequest("Invalid user ID.");
            }

            var cars = _carService.GetCarByOwner(userId);

            if (cars == null || cars.Count == 0)
            {
                return NotFound("Not found");
            }

            return Ok(cars);
        }

        [HttpGet("detail/{carId}")]
        public ActionResult<CarDetailDTO> GetCarDetailById(int carId)
        {
            if (carId <= 0)
            {
                return BadRequest("Invalid car ID.");
            }

            var carDetail = _carService.GetCarDetailById(carId);

            if (carDetail == null)
            {
                return NotFound("Car not found.");
            }

            return Ok(carDetail);
        }


        [HttpGet("real-time-status/{carId}")]
        public ActionResult<ChargingStatusDTO> GetChargingStatus(int carId)
        {
            var result = _carService.GetChargingStatusById(carId);
            if (result == null)
            {
                return NotFound(new { message = "Not found" });
            }

            return Ok(result);
        }


        [HttpGet]
        public ActionResult<List<ChargingHistoryDTO>> GetChargingHistory(
            [FromQuery] int carId,
            [FromQuery] DateTime? start,
            [FromQuery] DateTime? end,
            [FromQuery] int? chargingStationId)
        {
            var result = _carService.GetChargingHistory(carId, start, end, chargingStationId);
            if (result == null || result.Count == 0)
            {
                return NotFound(new { message = "Not found" });
            }

            return Ok(result);
        }
    }
}
