using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class UserCar
{
    public int UserId { get; set; }

    public int CarId { get; set; }

    public string? Role { get; set; }

    public bool? IsAllowedToCharge { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public virtual Car Car { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
