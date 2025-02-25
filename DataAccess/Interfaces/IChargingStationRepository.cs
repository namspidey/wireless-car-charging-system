using DataAccess.DTOs;

namespace DataAccess.Interfaces
{
    public interface IChargingStationRepository
    {
        List<ChargingStationDto>? GetAllStation(string? keyword);
        ChargingStationDto GetStationById(int stationId);
        void SaveStation(NewChargingStationDto s);
        void UpdateStation(UpdateChargingStationDto s);
        void DeleteStation(int stationId);
    }
}
