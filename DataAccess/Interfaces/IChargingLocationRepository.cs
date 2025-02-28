using DataAccess.Models;

namespace DataAccess.Interfaces
{
    public interface IChargingLocationRepository
    {
        StationLocation GetLocationById(int locationId);
    }
}
