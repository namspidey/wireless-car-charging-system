using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTO
{
    public class CarDetailDTO
    {
        public int CarId { get; set; }
        public string? CarName { get; set; }
        public string? LicensePlate { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? CarCreateAt { get; set; }
        public DateTime? CarUpdateAt { get; set; }

        public int CarModelId { get; set; }
        public string? Type { get; set; }
        public string? Color { get; set; }
        public int? SeatNumber { get; set; }
        public string? Brand { get; set; }
        public double? BatteryCapacity { get; set; }
        public double? MaxChargingPower { get; set; }
        public double? AverageRange { get; set; }
        public string? ChargingStandard { get; set; }
        public string? Img { get; set; }
        public DateTime? ModelCreateAt { get; set; }
        public DateTime? ModelUpdateAt { get; set; }
    }
}

