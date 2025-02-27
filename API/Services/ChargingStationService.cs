using DataAccess.DTOs;
using DataAccess.Interfaces;
using DataAccess.Repositories;

namespace API.Services
{
    public class ChargingStationService
    {
        private readonly IChargingStationRepository _repository;

        public ChargingStationService(IChargingStationRepository repository)
        {
            _repository = repository;
        }

        public PagedResult<ChargingStationDto> GetChargingStations(string? keyword, decimal userLat, decimal userLng, int page, int pageSize)
        {
            return _repository.GetAllStation(keyword, userLat, userLng, page, pageSize);
        }

        public ChargingStationDto GetChargingStationById(int stationId)
        {
            return _repository.GetStationById(stationId);
        }

        public void SaveChargingStation(NewChargingStationDto chargingStation)
        {
            _repository.SaveStation(chargingStation);
        }

        public void UpdateChargingStation(UpdateChargingStationDto chargingStation)
        {
            _repository.UpdateStation(chargingStation);
        }

        public void DeleteChargingStation(int stationId)
        {
            _repository.DeleteStation(stationId);
        }

    }
}
