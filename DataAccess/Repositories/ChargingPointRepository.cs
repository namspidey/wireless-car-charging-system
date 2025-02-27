using DataAccess.DTOs;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class ChargingPointRepository : IChargingPointRepository
    {
        private readonly WccsContext _context;

        public ChargingPointRepository(WccsContext context)
        {
            _context = context;
        }

        public List<ChargingPointDto> GetAllPointsByStation(int stationId)
        {
            var points = _context.ChargingPoints
                .Include(cp => cp.ChargingSessions)
                .Include(cp => cp.RealTimeData)             // Get Station + Location & Point
                .Where(cp => cp.StationId == stationId)     // Compare StationID with variable
                .AsNoTracking()
                .Select(cp => new ChargingPointDto
                {
                    ChargingPointId = cp.ChargingPointId,
                    StationId = cp.StationId,
                    ChargingPointName = cp.ChargingPointName,
                    Description = cp.Description,
                    Status = cp.Status,
                    MaxPower = cp.MaxPower,
                    CreateAt = cp.CreateAt,
                    UpdateAt = cp.UpdateAt,
                    MaxConsumPower = cp.MaxConsumPower
                })
                .ToList();

            return points;
        }

        public ChargingPointDto GetPointById(int pointId)
        {
            var point = _context.ChargingPoints
                .Include(cp => cp.ChargingSessions)
                .Include(cp => cp.RealTimeData)                 // Get Station + Location & Point
                .Where(cp => cp.ChargingPointId == pointId)     // Compare ChargingPointID with variable
                .AsNoTracking()
                .Select(cp => new ChargingPointDto
                {
                    ChargingPointId = cp.ChargingPointId,
                    StationId = cp.StationId,
                    ChargingPointName = cp.ChargingPointName,
                    Description = cp.Description,
                    Status = cp.Status,
                    MaxPower = cp.MaxPower,
                    CreateAt = cp.CreateAt,
                    UpdateAt = cp.UpdateAt,
                    MaxConsumPower = cp.MaxConsumPower
                })
                .FirstOrDefault();

            return point;
        }

        public void SaveChargingPoint(NewChargingPointDto cp)
        {
            var newPoint = new ChargingPoint
            {
                StationId = cp.StationId,
                ChargingPointName = cp.ChargingPointName,
                Description = cp.Description,
                Status = cp.Status,
                MaxPower = cp.MaxPower,
                CreateAt = cp.CreateAt,
                MaxConsumPower = cp.MaxConsumPower
            };

            _context.ChargingPoints.Add(newPoint);
            _context.SaveChanges();

        }

        public void UpdateChargingPoint(UpdateChargingPointDto cp)
        {
            var point = _context.ChargingPoints.Find(cp.ChargingPointId);
            if (point == null) throw new KeyNotFoundException("Charging Point not found");

            point.StationId = cp.StationId ?? point.StationId;
            point.ChargingPointName = cp.ChargingPointName ?? point.ChargingPointName;
            point.Description = cp.Description ?? point.Description;
            point.Status = cp.Status ?? point.Status;
            point.MaxPower = cp.MaxPower ?? point.MaxPower;
            point.UpdateAt = DateTime.UtcNow;
            point.MaxConsumPower = cp.MaxConsumPower ?? point.MaxConsumPower;

            _context.ChargingPoints.Update(point);
            _context.SaveChanges();
        }

        public void DeleteChargingPoint(int pointId)
        {
            var point = _context.ChargingPoints.Find(pointId);
            if (point == null) throw new KeyNotFoundException("Charging Point not found");

            _context.ChargingPoints.Remove(point);
            _context.SaveChanges();
        }

    }
}
