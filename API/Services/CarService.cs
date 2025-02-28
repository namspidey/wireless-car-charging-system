using DataAccess.DTO;
using DataAccess.Interfaces;
using DataAccess.Models;

namespace API.Services
{
    public class CarService
    {
        private readonly IMyCars _myCars;

        public CarService(IMyCars myCar)
        {
            _myCars = myCar;
        }

        public List<MyCarsDTO> GetCarByOwner( int userId)
        {
            return _myCars.getCarByOwner(userId);
        }

        public CarDetailDTO GetCarDetailById(int carId)
        {
            return _myCars.getCarDetailById(carId);
        }

        public ChargingStatusDTO GetChargingStatusById(int carId) {
            return _myCars.GetChargingStatusById(carId);
        }

        public List<ChargingHistoryDTO> GetChargingHistory(int carId, DateTime? start, DateTime? end, int? chargingStationId)
        {
            return _myCars.GetChargingHistory(carId, start, end, chargingStationId);
        }

        public void deleteCar(int carId)
        {
             _myCars.deleteCar(carId);
        }
    }
}
