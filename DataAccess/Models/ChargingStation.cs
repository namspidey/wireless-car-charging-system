using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class ChargingStation
{
    public int StationId { get; set; }

    public int? OwnerId { get; set; }

    public int? StationLocationId { get; set; }

    public string? StationName { get; set; }

    public string? Status { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public double? MaxConsumPower { get; set; }

    public virtual ICollection<ChargingPoint> ChargingPoints { get; set; } = new List<ChargingPoint>();

    public virtual User? Owner { get; set; }

    public virtual StationLocation? StationLocation { get; set; }
}
