using DataAccess.DTO;
using DataAccess.Interfaces;
using DataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories.CarRepo
{
    public class MyCarsRepo : IMyCars
    {
        private WccsContext _context;
        public MyCarsRepo()
        {
            _context = new WccsContext();
        }

        public List<MyCarsDTO> getCarByOwner(int userId)
        {
            var cars = (from uc in _context.UserCars
                        join c in _context.Cars on uc.CarId equals c.CarId
                        join cm in _context.CarModels on c.CarModelId equals cm.CarModelId
                        where uc.UserId == userId && uc.Role == "Owner" && c.IsDeleted == false
                        select new MyCarsDTO
                        {
                            CarId = c.CarId,
                            CarName = c.CarName,
                            LicensePlate = c.LicensePlate,
                            IsDeleted = c.IsDeleted,
                            Type = cm.Type,
                            Color = cm.Color,
                            Brand = cm.Brand,
                            Img = cm.Img
                        }).ToList();

            return cars;
        }

        public CarDetailDTO getCarDetailById(int carId)
        {
            var carDetail = (from c in _context.Cars
                             join cm in _context.CarModels on c.CarModelId equals cm.CarModelId
                             where c.CarId == carId && c.IsDeleted == false
                             select new CarDetailDTO
                             {
                                 CarId = c.CarId,
                                 CarName = c.CarName,
                                 LicensePlate = c.LicensePlate,
                                 IsDeleted = c.IsDeleted,
                                 CarCreateAt = c.CreateAt,
                                 CarUpdateAt = c.UpdateAt,
                                 CarModelId = cm.CarModelId,
                                 Type = cm.Type,
                                 Color = cm.Color,
                                 SeatNumber = cm.SeatNumber,
                                 Brand = cm.Brand,
                                 BatteryCapacity = cm.BatteryCapacity,
                                 MaxChargingPower = cm.MaxChargingPower,
                                 AverageRange = cm.AverageRange,
                                 ChargingStandard = cm.ChargingStandard,
                                 Img = cm.Img,
                                 ModelCreateAt = cm.CreateAt,
                                 ModelUpdateAt = cm.UpdateAt
                             }).FirstOrDefault();

            return carDetail;
        }

        public ChargingStatusDTO? GetChargingStatusById(int carId)
        {
            var query = from cs in _context.ChargingSessions
                        join cp in _context.ChargingPoints on cs.ChargingPointId equals cp.ChargingPointId
                        join s in _context.ChargingStations on cp.StationId equals s.StationId
                        join sl in _context.StationLocations on s.StationLocationId equals sl.StationLocationId
                        join rtd in _context.RealTimeData on cs.CarId equals rtd.CarId
                        where cs.Status == "Charging" && cs.CarId == carId
                        select new ChargingStatusDTO
                        {
                            SessionId = cs.SessionId,
                            CarId = cs.CarId,
                            ChargingPointId = cs.ChargingPointId,
                            StationId = s.StationId,
                            StationLocationId = s.StationLocationId,
                            StationName = s.StationName,
                            Address = sl.Address,
                            Status = cs.Status,
                            BatteryLevel = rtd.BatteryLevel
                        };

            return query.FirstOrDefault(); 
        }

        public List<ChargingHistoryDTO> GetChargingHistory(int carId, DateTime? start, DateTime? end, int? chargingStationId)
        {
            var query = from cs in _context.ChargingSessions
                        join c in _context.Cars on cs.CarId equals c.CarId
                        join cp in _context.ChargingPoints on cs.ChargingPointId equals cp.ChargingPointId
                        join s in _context.ChargingStations on cp.StationId equals s.StationId
                        join sl in _context.StationLocations on s.StationLocationId equals sl.StationLocationId
                        where cs.CarId == carId
                        select new ChargingHistoryDTO
                        {
                            SessionId = cs.SessionId,
                            CarId = cs.CarId,
                            CarName = c.CarName,
                            LicensePlate = c.LicensePlate,
                            ChargingPointId = cs.ChargingPointId,
                            StationName = s.StationName,
                            StationId = cp.StationId,
                            Address = sl.Address,
                            StartTime = cs.StartTime,
                            EndTime = cs.EndTime,
                            Cost = cs.Cost,
                            Status = cs.Status
                        };


            if (start.HasValue && end.HasValue)
            {
                query = query.Where(cs => cs.StartTime >= start.Value && (cs.EndTime <= end.Value ));
            }
            else
            {
                if (start.HasValue && !end.HasValue)
                {
                    query = query.Where(cs => cs.StartTime >= start.Value);
                }

                if (end.HasValue && !start.HasValue)
                {
                    query = query.Where(cs => cs.EndTime <= end.Value );
                }
            }


            if (chargingStationId.HasValue)
            {
                query = query.Where(cs => cs.StationId == chargingStationId.Value);
            }

            return query.OrderByDescending(cs => cs.StartTime).ToList();
        }

        public void deleteCar(int carId)
        {
            var car = _context.Cars.FirstOrDefault(c => c.CarId == carId);
            if (car != null)
            {
                car.IsDeleted = true; 
                _context.SaveChanges(); 
            }
        }
    }
}
