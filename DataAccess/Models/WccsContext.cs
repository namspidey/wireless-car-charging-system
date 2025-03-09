using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DataAccess.Models;

public partial class WccsContext : DbContext
{
    public WccsContext()
    {
    }

    public WccsContext(DbContextOptions<WccsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Balance> Balances { get; set; }

    public virtual DbSet<BalanceTransaction> BalanceTransactions { get; set; }

    public virtual DbSet<Car> Cars { get; set; }

    public virtual DbSet<CarModel> CarModels { get; set; }

    public virtual DbSet<Cccd> Cccds { get; set; }

    public virtual DbSet<ChargingPoint> ChargingPoints { get; set; }

    public virtual DbSet<ChargingSession> ChargingSessions { get; set; }

    public virtual DbSet<ChargingStation> ChargingStations { get; set; }

    public virtual DbSet<DriverLicense> DriverLicenses { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<RealTimeDatum> RealTimeData { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<StationLocation> StationLocations { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserCar> UserCars { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        if (!optionsBuilder.IsConfigured) { optionsBuilder.UseSqlServer(config.GetConnectionString("value")); }

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Balance>(entity =>
        {
            entity.HasKey(e => e.BalanceId).HasName("PK__balance__18188B5B4D6627A6");

            entity.ToTable("balance");

            entity.Property(e => e.BalanceId).HasColumnName("balance_id");
            entity.Property(e => e.Balance1).HasColumnName("balance");
            entity.Property(e => e.CreateAt).HasColumnName("create_at");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Balances)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__balance__user_id__5BE2A6F2");
        });

        modelBuilder.Entity<BalanceTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PK__balance___85C600AF13232AD0");

            entity.ToTable("balance_transactions");

            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.BalanceId).HasColumnName("balance_id");
            entity.Property(e => e.TransactionDate).HasColumnName("transaction_date");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("transaction_type");

            entity.HasOne(d => d.Balance).WithMany(p => p.BalanceTransactions)
                .HasForeignKey(d => d.BalanceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__balance_t__balan__5EBF139D");
        });

        modelBuilder.Entity<Car>(entity =>
        {
            entity.HasKey(e => e.CarId).HasName("PK__car__4C9A0DB3362C2BDF");

            entity.ToTable("car");

            entity.Property(e => e.CarId).HasColumnName("car_id");
            entity.Property(e => e.CarModelId).HasColumnName("car_model_id");
            entity.Property(e => e.CarName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("car_name");
            entity.Property(e => e.CreateAt).HasColumnName("create_at");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.LicensePlate)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("license_plate");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at");

            entity.HasOne(d => d.CarModel).WithMany(p => p.Cars)
                .HasForeignKey(d => d.CarModelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__car__car_model_i__45F365D3");
        });

        modelBuilder.Entity<CarModel>(entity =>
        {
            entity.HasKey(e => e.CarModelId).HasName("PK__car_mode__6F9B237722B73302");

            entity.ToTable("car_model");

            entity.Property(e => e.CarModelId).HasColumnName("car_model_id");
            entity.Property(e => e.AverageRange).HasColumnName("average_range");
            entity.Property(e => e.BatteryCapacity).HasColumnName("battery_capacity");
            entity.Property(e => e.Brand)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("brand");
            entity.Property(e => e.ChargingStandard)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("charging_standard");
            entity.Property(e => e.Color)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("color");
            entity.Property(e => e.CreateAt).HasColumnName("create_at");
            entity.Property(e => e.Img)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("img");
            entity.Property(e => e.MaxChargingPower).HasColumnName("max_charging_power");
            entity.Property(e => e.SeatNumber).HasColumnName("seat_number");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("type");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at");
        });

        modelBuilder.Entity<Cccd>(entity =>
        {
            entity.HasKey(e => e.CccdId).HasName("PK__cccd__E47A6DF1C970C5FF");

            entity.ToTable("cccd");

            entity.Property(e => e.CccdId).HasColumnName("cccd_id");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.CreateAt).HasColumnName("create_at");
            entity.Property(e => e.ImgBack)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("img_back");
            entity.Property(e => e.ImgBackPubblicId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("img_backPubblicId");
            entity.Property(e => e.ImgFront)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("img_front");
            entity.Property(e => e.ImgFrontPubblicId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("img_frontPubblicId");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Cccds)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__cccd__update_at__6754599E");
        });

        modelBuilder.Entity<ChargingPoint>(entity =>
        {
            entity.HasKey(e => e.ChargingPointId).HasName("PK__charging__D7F595375CAFD8F0");

            entity.ToTable("charging_point");

            entity.Property(e => e.ChargingPointId).HasColumnName("charging_point_id");
            entity.Property(e => e.ChargingPointName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("charging_point_name");
            entity.Property(e => e.CreateAt).HasColumnName("create_at");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.MaxPower).HasColumnName("max_power");
            entity.Property(e => e.StationId).HasColumnName("station_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at");

            entity.HasOne(d => d.Station).WithMany(p => p.ChargingPoints)
                .HasForeignKey(d => d.StationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__charging___stati__48CFD27E");
        });

        modelBuilder.Entity<ChargingSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PK__charging__69B13FDCA4AD7E99");

            entity.ToTable("charging_session");

            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.CarId).HasColumnName("car_id");
            entity.Property(e => e.ChargingPointId).HasColumnName("charging_point_id");
            entity.Property(e => e.Cost).HasColumnName("cost");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.EnergyConsumed).HasColumnName("energy_consumed");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Car).WithMany(p => p.ChargingSessions)
                .HasForeignKey(d => d.CarId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__charging___car_i__534D60F1");

            entity.HasOne(d => d.ChargingPoint).WithMany(p => p.ChargingSessions)
                .HasForeignKey(d => d.ChargingPointId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__charging___charg__5441852A");

            entity.HasOne(d => d.User).WithMany(p => p.ChargingSessions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__charging___user___5535A963");
        });

        modelBuilder.Entity<ChargingStation>(entity =>
        {
            entity.HasKey(e => e.StationId).HasName("PK__charging__44B370E94477D86C");

            entity.ToTable("charging_station");

            entity.Property(e => e.StationId).HasColumnName("station_id");
            entity.Property(e => e.CreateAt).HasColumnName("create_at");
            entity.Property(e => e.OwnerId).HasColumnName("owner_id");
            entity.Property(e => e.StationLocationId).HasColumnName("station_location_id");
            entity.Property(e => e.StationName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("station_name");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at");

            entity.HasOne(d => d.Owner).WithMany(p => p.ChargingStations)
                .HasForeignKey(d => d.OwnerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__charging___owner__4222D4EF");

            entity.HasOne(d => d.StationLocation).WithMany(p => p.ChargingStations)
                .HasForeignKey(d => d.StationLocationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__charging___stati__4316F928");
        });

        modelBuilder.Entity<DriverLicense>(entity =>
        {
            entity.HasKey(e => e.DriverLicenseId).HasName("PK__driver_l__5EB6C89FB70AF772");

            entity.ToTable("driver_license");

            entity.Property(e => e.DriverLicenseId).HasColumnName("driver_license_id");
            entity.Property(e => e.Class)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("class");
            entity.Property(e => e.Code)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.CreateAt).HasColumnName("create_at");
            entity.Property(e => e.ImgBack)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("img_back");
            entity.Property(e => e.ImgBackPubblicId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("img_backPubblicId");
            entity.Property(e => e.ImgFront)
                .HasMaxLength(1000)
                .IsUnicode(false)
                .HasColumnName("img_front");
            entity.Property(e => e.ImgFrontPubblicId)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("img_frontPubblicId");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.DriverLicenses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__driver_li__updat__6A30C649");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.FeedbackId).HasName("PK__feedback__7A6B2B8CB94AB16C");

            entity.ToTable("feedback");

            entity.Property(e => e.FeedbackId).HasColumnName("feedback_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.Message)
                .HasMaxLength(1000)
                .HasColumnName("message");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__feedback__user_i__6477ECF3");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__payment__ED1FC9EA00A3C5FE");

            entity.ToTable("payment");

            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.PaymentDate).HasColumnName("payment_date");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("payment_method");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("payment_status");
            entity.Property(e => e.SessionId).HasColumnName("session_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Session).WithMany(p => p.Payments)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__payment__session__59063A47");

            entity.HasOne(d => d.User).WithMany(p => p.Payments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__payment__user_id__5812160E");
        });

        modelBuilder.Entity<RealTimeDatum>(entity =>
        {
            entity.HasKey(e => e.DataId).HasName("PK__real_tim__F5A76B3BE958FB2B");

            entity.ToTable("real_time_data");

            entity.Property(e => e.DataId).HasColumnName("data_id");
            entity.Property(e => e.BatteryLevel).HasColumnName("battery_level");
            entity.Property(e => e.CarId).HasColumnName("car_id");
            entity.Property(e => e.ChargingPower).HasColumnName("charging_power");
            entity.Property(e => e.ChargingpointId).HasColumnName("chargingpoint_id");
            entity.Property(e => e.Temperature).HasColumnName("temperature");
            entity.Property(e => e.Timestamp).HasColumnName("timestamp");

            entity.HasOne(d => d.Car).WithMany(p => p.RealTimeData)
                .HasForeignKey(d => d.CarId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__real_time__car_i__4F7CD00D");

            entity.HasOne(d => d.Chargingpoint).WithMany(p => p.RealTimeData)
                .HasForeignKey(d => d.ChargingpointId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__real_time__charg__5070F446");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.TokenId).HasName("PK__refresh___CB3C9E17D53B9B20");

            entity.ToTable("refresh_tokens");

            entity.Property(e => e.TokenId).HasColumnName("token_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.Revoked).HasColumnName("revoked");
            entity.Property(e => e.Token)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("token");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__refresh_t__user___619B8048");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__roles__760965CC09F463F9");

            entity.ToTable("roles");

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<StationLocation>(entity =>
        {
            entity.HasKey(e => e.StationLocationId).HasName("PK__station___0CE32FE772924BBD");

            entity.ToTable("station_location");

            entity.Property(e => e.StationLocationId).HasColumnName("station_location_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("address");
            entity.Property(e => e.CreateAt).HasColumnName("create_at");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.Latitude)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("longitude");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__users__B9BE370F8B926579");

            entity.ToTable("users");

            entity.HasIndex(e => e.PhoneNumber, "UQ__users__A1936A6B23B9F6AA").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__users__AB6E6164998767B5").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("address");
            entity.Property(e => e.CreateAt).HasColumnName("create_at");
            entity.Property(e => e.Dob).HasColumnName("dob");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Fullname)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("fullname");
            entity.Property(e => e.Gender).HasColumnName("gender");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password_hash");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("phone_number");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.UpdateAt).HasColumnName("update_at");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__users__role_id__3F466844");
        });

        modelBuilder.Entity<UserCar>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.CarId }).HasName("PK__user_car__9D7797D456E7FB36");

            entity.ToTable("user_car");

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CarId).HasColumnName("car_id");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Car).WithMany(p => p.UserCars)
                .HasForeignKey(d => d.CarId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__user_car__car_id__4CA06362");

            entity.HasOne(d => d.User).WithMany(p => p.UserCars)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__user_car__user_i__4BAC3F29");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
