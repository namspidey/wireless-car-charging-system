using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs.CarDTO
{
    public class ChargingStatusDTO
    {
       
        public int? CarId { get; set; }

        public int? ChargingPointId { get; set; }

        public int StationId { get; set; }
        public int? StationLocationId { get; set; }

        public string? StationName { get; set; }

        public string Address { get; set; } = null!;

        public string LicensePlate { get; set; }

        public string? Status { get; set; }

        public string? BatteryLevel { get; set; }
        public string? ChargingPower { get; set; }

        public string? Temperature { get; set; }

        public string? Cost { get; set; }


        public string? Current { get; set; }
        //more info
        public string? Powerpoint { get; set; }
        public string? BatteryVoltage { get; set; }


        public DateTime? TimeMoment { get; set; }
        public string? ChargingTime { get; set; }
        public string? EnergyConsumed { get; set; }

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
