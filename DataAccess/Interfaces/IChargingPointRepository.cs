using DataAccess.DTOs.ChargingStation;
using DataAccess.Models;
using DataAccess.Repositories;

namespace DataAccess.Interfaces
{
    public interface IChargingPointRepository
    {
        PagedResult<ChargingPointDto>? GetAllPointsByStation(int stationId, int page, int pageSize);
        ChargingPointDto GetPointById(int pointId);
        //void SaveChargingPoint(NewChargingPointDto s);
        void UpdateChargingPoint(UpdateChargingPointDto s);
        void DeleteChargingPoint(int pointId);
    }
}
