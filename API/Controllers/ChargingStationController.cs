using API.Services;
using DataAccess.DTOs.ChargingStation;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChargingStationController : ControllerBase
    {
        private ChargingStationService _stationService;

        public ChargingStationController(ChargingStationService stationService)
        {
            _stationService = stationService;
        }

        [HttpGet]
        public ActionResult GetChargingStations(string? keyword, decimal userLat, decimal userLng, int page = 1, int pageSize = 2)
        {
            var stations = _stationService.GetChargingStations(keyword, userLat, userLng, page, pageSize);
            return new JsonResult(stations);
        }


        [HttpGet("Detail/{stationId}")]
        public ActionResult GetStationDetails(int stationId, int page, int pageSize)
        {
            var stationDetails = _stationService.GetStationDetails(stationId, page, pageSize);

            if (stationDetails == null)
            {
                return NotFound(new { message = "Station not found" });
            }

            return Ok(stationDetails);
        }

        [HttpPost("Add")]
        public IActionResult AddStation(NewChargingStationDto cs)
        {
            _stationService.SaveChargingStation(cs);
            return NoContent();
        }

        [HttpDelete("Delete/{stationId}")]
        public IActionResult DeleteStation(int stationId)
        {
            _stationService.DeleteChargingStation(stationId);
            return NoContent();
        }

        [HttpPut("Update")]
        public IActionResult UpdateStation(UpdateChargingStationDto s)
        {
            _stationService.UpdateChargingStation(s);
            return NoContent();
        }
    }
}
