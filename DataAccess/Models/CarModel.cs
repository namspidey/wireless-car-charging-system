using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class CarModel
{
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

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual ICollection<Car> Cars { get; set; } = new List<Car>();
}
