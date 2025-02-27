using DataAccess.DTOs;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class PagedResult<T>
    {
        public List<T> Data { get; set; }
        public int TotalPages { get; set; }
    }

    public class ChargingStationRepository : IChargingStationRepository
    {
        private readonly WccsContext _context;

        public ChargingStationRepository(WccsContext context)
        {
            _context = context;
        }

        public PagedResult<ChargingStationDto> GetAllStation(string? keyword, decimal? userLat, decimal? userLng, int page, int pageSize)
        {
            // Lấy tất cả dữ liệu Stationn
            var query = _context.ChargingStations
                .Include(cs => cs.StationLocation)
                .Include(cs => cs.ChargingPoints)
                .AsQueryable();

            // Tìm kiếm theo từ khóa (name hoặc location)
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string lowerKeyword = keyword.ToLower();
                query = query.Where(cs =>
                    (!string.IsNullOrEmpty(cs.StationName) && cs.StationName.ToLower().Contains(lowerKeyword)) ||
                    (cs.StationLocation != null && cs.StationLocation.Address.ToLower().Contains(lowerKeyword))
                );
            }          

            // Chuyển dữ liệu sang DTO
            var stationList = query.AsNoTracking()
                            .Select(cs => new ChargingStationDto
                            {
                                StationId = cs.StationId,
                                Owner = cs.Owner.Fullname,
                                StationName = cs.StationName,
                                Status = cs.Status,
                                Address = cs.StationLocation.Address,
                                Longtitude = cs.StationLocation.Longitude,
                                Latitude = cs.StationLocation.Latitude,
                                TotalPoint = cs.ChargingPoints.Count(), 
                                AvailablePoint = cs.ChargingPoints.Count(cp => cp.Status == "Available"),
                                CreateAt = cs.CreateAt,
                                UpdateAt = cs.UpdateAt,
                                MaxConsumPower = cs.MaxConsumPower
                            })
                            .ToList();

            // Tính khoảng cách nếu có thông tin vị trí người dùng
            if (userLat.HasValue && userLng.HasValue)
            {
                foreach (var station in stationList)
                {
                    station.Distance = GetDistance(userLat.Value, userLng.Value, station.Latitude, station.Longtitude);
                }

                // Sắp xếp theo khoảng cách (gần nhất trước)
                stationList = stationList.OrderBy(s => s.Distance).ToList();
            }

            // Tính tổng số trang
            int totalRecords = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            // Phân trang (chỉ lấy dữ liệu của trang hiện tại)
            var data = stationList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<ChargingStationDto> { Data = data, TotalPages = totalPages };
        }

        // Hàm tính khoảng cách giữa hai điểm theo công thức Haversine
        private double GetDistance(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            const double R = 6371; // Bán kính Trái Đất (km)
            double dLat = (double)(lat2 - lat1) * Math.PI / 180;
            double dLon = (double)(lon2 - lon1) * Math.PI / 180;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos((double)lat1 * Math.PI / 180) * Math.Cos((double)lat2 * Math.PI / 180) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c; // Khoảng cách (km)
        }

        public ChargingStationDto? GetStationById(int stationId)
        {
            var station = _context.ChargingStations
                .Include(cs => cs.StationLocation)
                .Include(cs => cs.ChargingPoints)           // Get Station + Location & Point
                .Where(cs => cs.StationId == stationId)     // Compare StationID with variable
                .AsNoTracking()
                .Select(cs => new ChargingStationDto
                {
                    StationId = cs.StationId,
                    Owner = cs.Owner.Fullname,
                    StationName = cs.StationName,
                    Status = cs.Status,
                    Address = cs.StationLocation.Address,
                    Longtitude = cs.StationLocation.Longitude,
                    Latitude = cs.StationLocation.Latitude,
                    CreateAt = cs.CreateAt,
                    UpdateAt = cs.UpdateAt,
                    MaxConsumPower = cs.MaxConsumPower
                })
                .FirstOrDefault(); // Get first object

            return station;
        }

        public void SaveStation(NewChargingStationDto s)
        {
            var newStation = new ChargingStation
            {
                OwnerId = s.OwnerId,
                StationLocationId = s.StationLocationId,
                StationName = s.StationName,
                Status = s.Status,
                CreateAt = DateTime.UtcNow,
                MaxConsumPower = s.MaxConsumPower
            };

            _context.ChargingStations.Add(newStation);
            _context.SaveChanges();
        }


        public void UpdateStation(UpdateChargingStationDto s)
        {
            var station = _context.ChargingStations.Find(s.StationId);
            if (station == null) throw new KeyNotFoundException("Station not found");

            station.OwnerId = s.OwnerId ?? station.OwnerId;
            station.StationLocationId = s.StationLocationId ?? station.StationLocationId;
            station.StationName = s.StationName ?? station.StationName;
            station.Status = s.Status ?? station.Status;
            station.MaxConsumPower = s.MaxConsumPower ?? station.MaxConsumPower;
            station.UpdateAt = DateTime.UtcNow;

            _context.ChargingStations.Update(station);
            _context.SaveChanges();
        }


        public void DeleteStation(int stationId)
        {
            var station = _context.ChargingStations.Find(stationId);
            if (station == null) throw new KeyNotFoundException("Station not found");

            _context.ChargingStations.Remove(station);
            _context.SaveChanges();
        }

    }
}
