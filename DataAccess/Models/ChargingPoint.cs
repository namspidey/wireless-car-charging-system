using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class ChargingPoint
{
    public int ChargingPointId { get; set; }

    public int? StationId { get; set; }

    public string? ChargingPointName { get; set; }

    public string? Description { get; set; }

    public string? Status { get; set; }

    public double? MaxPower { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public double? MaxConsumPower { get; set; }

    public virtual ICollection<ChargingSession> ChargingSessions { get; set; } = new List<ChargingSession>();

    public virtual ICollection<RealTimeDatum> RealTimeData { get; set; } = new List<RealTimeDatum>();

    public virtual ChargingStation? Station { get; set; }
}
