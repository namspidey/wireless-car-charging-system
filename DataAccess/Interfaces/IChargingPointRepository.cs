using DataAccess.Models;

namespace DataAccess.Interfaces
{
    public interface IChargingPointRepository
    {
        List<ChargingPoint> GetAllPointsByStation(int stationId);
        ChargingPoint GetPointById(int pointId);
    }
}
