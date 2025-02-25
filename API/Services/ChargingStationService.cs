using DataAccess.DTOs;
using DataAccess.Interfaces;

namespace API.Services
{
    public class ChargingStationService
    {
        private readonly IChargingStationRepository _repository;

        public ChargingStationService(IChargingStationRepository repository)
        {
            _repository = repository;
        }

        public List<ChargingStationDto> GetChargingStations(string? keyword)
        {
            return _repository.GetAllStation(keyword);
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
