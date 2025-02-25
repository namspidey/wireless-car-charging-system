namespace DataAccess.DTOs
{
    public class ChargingStationDto
    {
        public int StationId { get; set; }

        public int? OwnerId { get; set; }

        public int? StationLocationId { get; set; }

        public string? StationName { get; set; }

        public string? Status { get; set; }

        public DateTime? CreateAt { get; set; }

        public DateTime? UpdateAt { get; set; }

        public double? MaxConsumPower { get; set; }
    }

    public class NewChargingStationDto
    {
        public int? OwnerId { get; set; }

        public int? StationLocationId { get; set; }

        public string? StationName { get; set; }

        public string? Status { get; set; }

        public DateTime? CreateAt { get; set; }

        public DateTime? UpdateAt { get; set; }

        public double? MaxConsumPower { get; set; }
    }

    public class UpdateChargingStationDto
    {
        public int StationId { get; set; }

        public int? OwnerId { get; set; }

        public int? StationLocationId { get; set; }

        public string? StationName { get; set; }

        public string? Status { get; set; }

        public DateTime? UpdateAt { get; set; }

        public double? MaxConsumPower { get; set; }
    }
}
