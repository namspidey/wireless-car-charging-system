using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DTOs.ChargingStation
{
    public class ChargingPointDto
    {
        public int ChargingPointId { get; set; }
        public string? ChargingPointName { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public double? MaxPower { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }
        public double? MaxConsumPower { get; set; }
    }

    public class NewChargingPointDto
    {
        public int? StationId { get; set; }

        public string? ChargingPointName { get; set; }

        public string? Description { get; set; }

        public string? Status { get; set; }

        public double? MaxPower { get; set; }

        public DateTime? CreateAt { get; set; }

        public double? MaxConsumPower { get; set; }
    }

    public class UpdateChargingPointDto
    {
        public int ChargingPointId { get; set; }

        public int? StationId { get; set; }

        public string? ChargingPointName { get; set; }

        public string? Description { get; set; }

        public string? Status { get; set; }

        public double? MaxPower { get; set; }

        public DateTime? UpdateAt { get; set; }

        public double? MaxConsumPower { get; set; }
    }
}
