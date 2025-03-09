using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int UserId { get; set; }

    public int SessionId { get; set; }

    public double? Amount { get; set; }

    public string? PaymentMethod { get; set; }

    public string? PaymentStatus { get; set; }

    public DateTime? PaymentDate { get; set; }

    public virtual ChargingSession Session { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
