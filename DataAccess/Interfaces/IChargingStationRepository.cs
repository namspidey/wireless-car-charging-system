using DataAccess.DTOs.ChargingStation;
using DataAccess.Repositories;

namespace DataAccess.Interfaces
{
    public interface IChargingStationRepository
    {
        PagedResult<ChargingStationDto>? GetAllStation(string? keyword, decimal? userLat, decimal? userLng, int page, int pageSize);
        ChargingStationDto GetStationById(int stationId);
        void SaveStation(NewChargingStationDto s);
        void UpdateStation(UpdateChargingStationDto s);
        void DeleteStation(int stationId);
    }
}
