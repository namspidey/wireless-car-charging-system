using API.Services;
using DataAccess.DTOs;
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
        public ActionResult getChargingStation(string? keyword)
        {
            var stations = _stationService.GetChargingStations(keyword);
            return new JsonResult(stations);
        }

        [HttpGet("Detail/{stationId}")]
        public ActionResult getChargingStationById(int stationId)
        {
            var station = _stationService.GetChargingStationById(stationId);
            return new JsonResult(station);
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
