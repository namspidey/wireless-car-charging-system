using DataAccess.DTOs;
using DataAccess.Models;

namespace DataAccess.Interfaces
{
    public interface IChargingPointRepository
    {
        List<ChargingPointDto> GetAllPointsByStation(int stationId);
        ChargingPointDto GetPointById(int pointId);
        void SaveChargingPoint(NewChargingPointDto s);
        void UpdateChargingPoint(UpdateChargingPointDto s);
        void DeleteChargingPoint(int pointId);
    }
}
