using DataAccess.DTOs.CarDTO;
using DataAccess.DTOs;
using DataAccess.DTOs.UserDTO;
using DataAccess.Interfaces;
using DataAccess.Models;
using DataAccess.Repositories.StationRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;

namespace DataAccess.Repositories.CarRepo
{
    public class MyCarsRepo : IMyCars
    {
        private WccsContext _context;
        public MyCarsRepo(WccsContext context)
        {
            _context = context;
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
                            Img = cm.Img,
                            Status = c.Status
                        }).ToList();

            return cars;
        }

        public CarDetailDTO getCarDetailById(int carId)
        {
            var carDetail = (from c in _context.Cars
                             join cm in _context.CarModels on c.CarModelId equals cm.CarModelId
                             join uc in _context.UserCars on c.CarId equals uc.CarId
                             join u in _context.Users on uc.UserId equals u.UserId
                             where c.CarId == carId
                                 && c.IsDeleted == false
                                 && c.Status == "APPROVED"
                                 && uc.Role == "Owner"
                             select new CarDetailDTO
                             {
                                 CarId = c.CarId,
                                 CarName = c.CarName,
                                 LicensePlate = c.LicensePlate,
                                 IsDeleted = c.IsDeleted,
                                 CarImgFront = c.ImgFront,
                                 CarImgBack = c.ImgBack,
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
                                 ModelUpdateAt = cm.UpdateAt,                  
                                 OwnerName = u.Fullname,
                                 OwnerAddress = u.Address
                             }).FirstOrDefault();

            return carDetail;
        }


        public ChargingStatusDTO? GetChargingStatusById(int carId)
        {
                    var latestTime = _context.RealTimeData
            .Where(rtd =>  rtd.CarId == carId)
            .Max(rtd => rtd.TimeMoment);

            var query = (from cp in _context.ChargingPoints
                         join s in _context.ChargingStations on cp.StationId equals s.StationId
                         join sl in _context.StationLocations on s.StationLocationId equals sl.StationLocationId
                         join rtd in _context.RealTimeData on cp.ChargingPointId equals rtd.ChargingpointId
                         where  rtd.CarId == carId && rtd.TimeMoment == latestTime
                         select new ChargingStatusDTO
                         {
                             CarId = rtd.CarId,
                             ChargingPointId = rtd.ChargingpointId,
                             StationId = s.StationId,
                             StationLocationId = s.StationLocationId,
                             StationName = s.StationName,
                             Address = sl.Address,
                             Status = rtd.Status,
                             LicensePlate = rtd.LicensePlate,
                             BatteryLevel = rtd.BatteryLevel,
                             ChargingPower = rtd.ChargingPower,
                             Temperature = rtd.Temperature,
                             Cost = rtd.Cost,
                             Powerpoint = rtd.Powerpoint,
                             BatteryVoltage = rtd.BatteryVoltage,
                             Current = rtd.ChargingCurrent,
                             TimeMoment = rtd.TimeMoment,
                             ChargingTime = rtd.ChargingTime,
                             EnergyConsumed = rtd.EnergyConsumed
                         })
                         .FirstOrDefault();

            return query;
        }


        public List<ChargingHistoryDTO> GetChargingHistory(int carId, DateTime? start, DateTime? end, int? chargingStationId, int page = 1, int pageSize = 10)
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
                query = query.Where(cs => cs.StartTime >= start.Value && cs.EndTime <= end.Value);
            }
            else if (start.HasValue)
            {
                query = query.Where(cs => cs.StartTime >= start.Value);
            }
            else if (end.HasValue)
            {
                query = query.Where(cs => cs.EndTime <= end.Value);
            }

            if (chargingStationId.HasValue)
            {
                query = query.Where(cs => cs.StationId == chargingStationId.Value);
            }

            return query
                .OrderByDescending(cs => cs.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }




        public bool deleteCar(int carId)
        {
            var car = _context.Cars.FirstOrDefault(c => c.CarId == carId);
            if (car != null)
            {
                car.IsDeleted = true;
                _context.SaveChanges();
                return true;
            }
            return false; 
        }

        public List<CarModel> getCarModels(string? search)
        {
            var carModels = _context.CarModels.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();

                carModels = carModels.Where(c =>
                    (c.Type != null && c.Type.ToLower().Contains(search)) ||
                    (c.Color != null && c.Color.ToLower().Contains(search)) ||
                    (c.Brand != null && c.Brand.ToLower().Contains(search))
                );
            }

            return carModels.ToList();
        }

        public bool checkDuplicateLicensePlate(string licensePlate)
        {
            bool isDuplicate = _context.Cars
                    .Any(c => c.LicensePlate.ToLower() == licensePlate.ToLower() && (c.IsDeleted ?? false) == false);

            return isDuplicate;
            
        }

        public async Task addCar(Car request, int userId)
        {
            //code here
            var carModelEntity = _context.CarModels.FirstOrDefault(cm => cm.CarModelId == request.CarModelId);
            if (carModelEntity == null)
            {
                throw new Exception("Car model không tồn tại.");
            }

            var carNameIn = request.CarName;
            if (string.IsNullOrWhiteSpace(request.CarName))
            {
                var brand = string.IsNullOrWhiteSpace(carModelEntity.Brand) ? "" : carModelEntity.Brand.Trim();
                var type = string.IsNullOrWhiteSpace(carModelEntity.Type) ? "" : carModelEntity.Type.Trim();
                carNameIn = $"{brand} {type}".Trim();
            }

            try
                {
                    var car = new Car
                    {
                        CarModelId = request.CarModelId,
                        CarName = carNameIn,
                        LicensePlate = request.LicensePlate,
                        ImgFront = request.ImgFront,
                        ImgBack = request.ImgBack,
                        ImgFrontPubblicId = request.ImgFrontPubblicId,
                        ImgBackPubblicId = request.ImgBackPubblicId,
                        Status = request.Status,
                        IsDeleted = false,
                        CreateAt = DateTime.UtcNow,
                        UpdateAt = DateTime.UtcNow
                    };

                    _context.Cars.Add(car);
                    _context.SaveChanges();

                    var userCar = new UserCar
                    {
                        UserId = userId,
                        CarId = car.CarId, 
                        Role = "Owner",      
                        IsAllowedToCharge = true,
                        StartDate = DateTime.UtcNow
                    };

                    _context.UserCars.Add(userCar);
                    _context.SaveChanges();

                    var document = new DocumentReview
                    {
                        UserId = userId,
                        CarId = car.CarId,
                        ReviewType = "CAR_LICENSE",
                        Status = "PENDING",
                        CreateAt = DateTime.UtcNow,
                        UpdateAt = DateTime.UtcNow
                    };
                     _context.DocumentReviews.Add(document);
                    _context.SaveChanges();
                //transaction.Commit();
            }
                catch (Exception ex)
                {
                   // transaction.Rollback();
                    throw new Exception("Error add car: " + ex.Message);
                }
            
        }

        public void editCar(int carModel, int carId, string licensePlate, string carName)
        {
            var car = _context.Cars.FirstOrDefault(c => c.CarId == carId);
            var carModelEntity = _context.CarModels.FirstOrDefault(cm => cm.CarModelId == carModel);
            if (carModelEntity == null)
            {
                throw new Exception("Car model không tồn tại.");
            }

            var carNameIn = carName;
            if (string.IsNullOrWhiteSpace(carName))
            {
                var brand = string.IsNullOrWhiteSpace(carModelEntity.Brand) ? "" : carModelEntity.Brand.Trim();
                var type = string.IsNullOrWhiteSpace(carModelEntity.Type) ? "" : carModelEntity.Type.Trim();
                carNameIn = $"{brand} {type}".Trim();
            }
            if (car != null)
            {
                car.CarModelId = carModel;
                car.LicensePlate = licensePlate;
                car.CarName = carNameIn;
                car.UpdateAt = DateTime.Now;
                _context.SaveChanges();
            }
        }

        public async Task SendRentRequestForRent(UserCar userCar)
        {
            await _context.UserCars.AddAsync(userCar);
            await _context.SaveChangesAsync();
        }

        public async Task<List<RentConfirmDto>> GetRentRequest(int driverId)
        {
            var currentDate = DateTime.Now;

            var rentRequests = await (from uc in _context.UserCars
                                      join c in _context.Cars on uc.CarId equals c.CarId
                                      join cm in _context.CarModels on c.CarModelId equals cm.CarModelId
                                      join owner in _context.UserCars on c.CarId equals owner.CarId
                                      join ownerUser in _context.Users on owner.UserId equals ownerUser.UserId
                                      where uc.UserId == driverId && uc.Role == "Renter" // Lọc người thuê
                                            && owner.Role == "Owner" // Lọc đúng chủ xe
                                            && uc.StartDate <= currentDate && uc.EndDate >= currentDate // Lọc yêu cầu trong khoảng thời gian hiện tại
                                      select new RentConfirmDto
                                      {
                                          DriverId = uc.UserId,
                                          OwnerId = owner.UserId,
                                          OwnerName = ownerUser.Fullname,
                                          OwnerPhone = ownerUser.PhoneNumber,
                                          CarId = c.CarId,
                                          LicensePlate = c.LicensePlate,
                                          IsAllowedToCharge = uc.IsAllowedToCharge,
                                          Type = cm.Type,
                                          Color = cm.Color,
                                          Brand = cm.Brand,
                                          StartDate = uc.StartDate,
                                          EndDate = uc.EndDate
                                      }).Distinct().ToListAsync();

            return rentRequests;
        }


        public async Task<UserCar?> GetUserCarAsync(int userId, int carId, string role)
        {
            return await _context.UserCars
            .FirstOrDefaultAsync(uc => uc.UserId == userId && uc.CarId == carId && uc.Role == role);
        }

        public async Task<bool> UpdateIsAllowedToChargeAsync(int userId, int carId, string role)
        {
            var userCar = await GetUserCarAsync(userId, carId, role);
            if (userCar == null) return false;

            userCar.IsAllowedToCharge = true;
            _context.UserCars.Update(userCar);
            await _context.SaveChangesAsync();
            return true;
        }

        public PagedResult<CarDetailDTO> GetAllCars(string? search, string? type, string? brand, bool? status, int page, int pageSize)
        {
            var query = _context.Cars
                .Where(f =>
                    (string.IsNullOrEmpty(search) || f.CarName.Contains(search) || f.LicensePlate.Contains(search)) &&
                    (string.IsNullOrEmpty(type) || f.CarModel.Type.Contains(type)) &&
                    (string.IsNullOrEmpty(brand) || f.CarModel.Brand.Contains(brand)) &&
                    (status == null || f.IsDeleted == status)
                )
                .Select(f => new CarDetailDTO
                {
                    CarId = f.CarId,
                    CarName = f.CarName,
                    LicensePlate = f.LicensePlate,
                    IsDeleted = f.IsDeleted,
                    Owner = f.UserCars
                        .Where(uc => uc.Role == "Owner")
                        .Select(uc => uc.User.Fullname)
                        .FirstOrDefault(),
                    Email = f.UserCars
                        .Where(uc => uc.Role == "Owner")
                        .Select(uc => uc.User.Email)
                        .FirstOrDefault(),
                    Type = f.CarModel.Type,
                    Color = f.CarModel.Color,
                    SeatNumber = f.CarModel.SeatNumber,
                    Brand = f.CarModel.Brand,
                    BatteryCapacity = f.CarModel.BatteryCapacity,
                    MaxChargingPower = f.CarModel.MaxChargingPower,
                    AverageRange = f.CarModel.AverageRange,
                    ChargingStandard = f.CarModel.ChargingStandard,
                    Img = f.CarModel.Type,
                });

            int totalCount = query.Count(); ;
            var data = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResult<CarDetailDTO>(data, totalCount, pageSize);
        }

        public List<string?> GetAllBrands()
        {
            return _context.CarModels
                .Select(m => m.Brand)
                .Distinct()
                .ToList();
        }

        public List<string?> GetAllTypes()
        {
            return _context.CarModels
                .Select(m => m.Type)
                .Distinct()
                .ToList();
        }

        public async Task<ChargingSession> AddChargingSession(ChargingSession session)
        {
            _context.ChargingSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task<int?> GetCurrentDriverByCarId(int carId)
        {
            var result = await _context.UserCars
           .Where(uc => uc.CarId == carId && uc.IsAllowedToCharge == true)
           .OrderBy(uc => uc.Role == "Renter" ? 1 : uc.Role == "Owner" ? 2 : 3)
           .Select(uc => (int?)uc.UserId)
           .FirstOrDefaultAsync();

            return result;
        }

        //public  List<ChargingSession> GetChargingHistoryByCarId(int carId)
        //{
        //            return  _context.ChargingSessions
        //        .Where(cs => cs.CarId == carId)
        //        .OrderByDescending(cs => cs.StartTime)
        //        .ToList();
        //}

        public List<CarMonthlyStatDTO> GetCarStats(int carId, int year)
        {
            var sessions = _context.ChargingSessions
                .Where(x => x.CarId == carId
                            && x.StartTime.HasValue
                            && x.StartTime.Value.Year == year)
                .ToList();

            var monthlyStats = Enumerable.Range(1, 12).Select(month =>
            {
                var monthSessions = sessions.Where(s => s.StartTime!.Value.Month == month).ToList();

                double totalTimeMinutes = monthSessions
                    .Where(s => s.EndTime.HasValue)
                    .Sum(s => (s.EndTime.Value - s.StartTime!.Value).TotalMinutes);

                return new CarMonthlyStatDTO
                {
                    Month = month,
                    SessionCount = monthSessions.Count,
                    TotalCost = monthSessions.Sum(s => s.Cost ?? 0),
                    TotalEnergy = monthSessions.Sum(s => s.EnergyConsumed ?? 0),
                    TotalTime = totalTimeMinutes,
                    AverageTime = monthSessions.Count == 0 ? 0 : totalTimeMinutes / monthSessions.Count
                };
            }).ToList();

            return monthlyStats;
        }

        public bool CheckDuplicateLicensePlateForEdit(int carId, string newLicensePlate)
        {
            var existingCar = _context.Cars
            .FirstOrDefault(c => c.LicensePlate == newLicensePlate && c.CarId != carId && c.IsDeleted != true);

            return existingCar != null;
        }

        public async Task<bool> IsCarBeingRentedAsync(int carId)
        {
            var currentTime = DateTime.UtcNow;

            return await _context.UserCars
                .AnyAsync(uc =>
                    uc.CarId == carId &&
                    uc.Role == "Renter" &&
                    uc.IsAllowedToCharge == true &&
                    uc.StartDate <= currentTime &&
                    (uc.EndDate == null || uc.EndDate >= currentTime));
        }

        public  bool IsAllowToAccess(int carId, int userId)
        {
            var owner =  _context.UserCars
        .FirstOrDefault(uc => uc.CarId == carId && uc.Role == "Owner");

            if (owner == null)
                return false;

            return owner.UserId == userId;
        }

        public bool IsRenterViewAnalysis(int carId, int renterId)
        {
            var now = DateTime.Now;

            return _context.UserCars.Any(uc =>
                uc.CarId == carId &&
                uc.UserId == renterId &&
                uc.Role == "Renter" &&
                uc.IsAllowedToCharge == true &&
                uc.StartDate <= now &&
                uc.EndDate >= now
            );
        }

        public async Task ChangeCarStatusAsync(int? carId, string newStatus)
        {
            if (string.IsNullOrEmpty(newStatus))
            {
                throw new ArgumentException("Trạng thái không thể trống", nameof(newStatus));
            }
            if (carId <= 0)
            {
                throw new ArgumentException("ID người dùng không hợp lệ", nameof(carId));
            }
            var car = await _context.Cars.FindAsync(carId);
            if (car != null)
            {
                car.Status = newStatus;
                _context.Cars.Update(car);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateExpiredRentalsAsync()
        {
            var now = DateTime.Now;
            var expiredRents = await _context.UserCars
                .Where(uc => uc.Role == "Renter" && uc.EndDate <= now && uc.IsAllowedToCharge == true)
                .ToListAsync();

            foreach (var rent in expiredRents)
            {
                rent.IsAllowedToCharge = false;
            }

            if (expiredRents.Any())
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Car?> GetCarById(int id)
        {
            return await _context.Cars
                .Include(c => c.CarModel)
                .Include(c => c.UserCars).ThenInclude(uc => uc.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CarId == id);
        }
    }
}
