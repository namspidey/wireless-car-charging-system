using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs
{
    public class ChargingStatusDTO
    {
        public int SessionId { get; set; }

        public int? CarId { get; set; }

        public int? ChargingPointId { get; set; }

        public int StationId { get; set; }
        public int? StationLocationId { get; set; }

        public string? StationName { get; set; }

        public string Address { get; set; } = null!;

        public string? Status { get; set; }

        public double? BatteryLevel { get; set; }

    }

    public class ChargingHistoryDTO
    {
        public int SessionId { get; set; }
        public int? CarId { get; set; }
        public string? CarName { get; set; }
        public string? LicensePlate { get; set; }

        public int? ChargingPointId { get; set; }
        public int? StationId { get; set; }
        public string? StationName { get; set; }
        public string Address { get; set; } = null!;
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double? Cost { get; set; }
        public string Status { get; set; } = null!;
        
    }
}
