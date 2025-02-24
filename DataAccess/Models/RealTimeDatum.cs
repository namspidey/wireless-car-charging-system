using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class RealTimeDatum
{
    public int DataId { get; set; }

    public int CarId { get; set; }

    public int ChargingpointId { get; set; }

    public double? BatteryLevel { get; set; }

    public double? ChargingPower { get; set; }

    public double? Temperature { get; set; }

    public DateTime? Timestamp { get; set; }

    public virtual Car Car { get; set; } = null!;

    public virtual ChargingPoint Chargingpoint { get; set; } = null!;
}
