using DataAccess.DTOs;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class ChargingStationRepository : IChargingStationRepository
    {
        private readonly WccsContext _context;

        public ChargingStationRepository(WccsContext context)
        {
            _context = context;
        }

        public List<ChargingStationDto> GetAllStation(string? keyword)
        {
            var query = _context.ChargingStations       // Get Station + Location & Point
                .Include(cs => cs.StationLocation)
                .Include(cs => cs.ChargingPoints)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))    // Check keyword ! null or empty
            {
                string lowerKeyword = keyword.ToLower();
                query = query.Where(cs =>
                    (!string.IsNullOrEmpty(cs.StationName) && cs.StationName.ToLower().Contains(lowerKeyword)) ||   // Compare with station name
                    (cs.StationLocation != null && cs.StationLocation.Address.ToLower().Contains(lowerKeyword))     // Compare with station location
                );
            }

            // If keyword = null or empty, return all station
            // If keyword ! null or empty, return found station
            return query.AsNoTracking()
                        .Select(cs => new ChargingStationDto
                        {
                            StationId = cs.StationId,
                            OwnerId = cs.OwnerId,
                            StationLocationId = cs.StationLocationId,
                            StationName = cs.StationName,
                            Status = cs.Status,
                            CreateAt = cs.CreateAt,
                            UpdateAt = cs.UpdateAt,
                            MaxConsumPower = cs.MaxConsumPower
                        })
                        .ToList();
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
                    OwnerId = cs.OwnerId,
                    StationLocationId = cs.StationLocationId,
                    StationName = cs.StationName,
                    Status = cs.Status,
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
