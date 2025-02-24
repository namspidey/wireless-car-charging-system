using System;
using System.Collections.Generic;

namespace DataAccess.Models;

public partial class Car
{
    public int CarId { get; set; }

    public int? CarModelId { get; set; }

    public string? CarName { get; set; }

    public string? LicensePlate { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual CarModel? CarModel { get; set; }

    public virtual ICollection<ChargingSession> ChargingSessions { get; set; } = new List<ChargingSession>();

    public virtual ICollection<RealTimeDatum> RealTimeData { get; set; } = new List<RealTimeDatum>();

    public virtual ICollection<UserCar> UserCars { get; set; } = new List<UserCar>();
}
