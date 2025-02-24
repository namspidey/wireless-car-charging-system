using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class StationLocation
{
    public int StationLocationId { get; set; }

    public string Address { get; set; } = null!;

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public string? Description { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual ICollection<ChargingStation> ChargingStations { get; set; } = new List<ChargingStation>();
}
