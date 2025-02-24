using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class ChargingSession
{
    public int SessionId { get; set; }

    public int? CarId { get; set; }

    public int? ChargingPointId { get; set; }

    public int? UserId { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public double? EnergyConsumed { get; set; }

    public double? Cost { get; set; }

    public string? Status { get; set; }

    public virtual Car? Car { get; set; }

    public virtual ChargingPoint? ChargingPoint { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual User? User { get; set; }
}
