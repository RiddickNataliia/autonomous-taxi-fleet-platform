namespace NovaDrive.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }
        public DbSet<User>            Users            { get; set; }
        public DbSet<Passenger>       Passengers       { get; set; }
        public DbSet<Vehicle>         Vehicles         { get; set; }
        public DbSet<Ride>            Rides            { get; set; }
        public DbSet<Transaction>     Transactions     { get; set; }
        public DbSet<MaintenanceLog>  MaintenanceLogs  { get; set; }
        public DbSet<SupportTicket>   SupportTickets   { get; set; }
        public DbSet<DiscountCode>    DiscountCodes    { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Passenger>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.HasOne(p => p.User)
                .WithOne(u => u.PassengerProfile)
                .HasForeignKey<Passenger>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade); 
        });

        modelBuilder.Entity<Ride>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.NetAmount).HasPrecision(18, 2);
            entity.Property(r => r.VatAmount).HasPrecision(18, 2);
            entity.Property(r => r.FinalPrice).HasPrecision(18, 2).IsRequired();
            entity.Property(r => r.LoyaltyDiscountApplied).HasPrecision(18, 2);
            entity.Property(r => r.CodeDiscountApplied).HasPrecision(18, 2);
            entity.Property(r => r.Status)
                .HasConversion<string>() 
                .IsRequired();
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(r => r.PassengerId)
                .OnDelete(DeleteBehavior.SetNull);
        });


            modelBuilder.Entity<Vehicle>(entity =>
            {
                int nextYear = DateTime.UtcNow.Year + 1;
                entity.ToTable(t => t.HasCheckConstraint("CK_Vehicle_Year", $"\"YearOfManufacture\" > 1900 AND \"YearOfManufacture\" <= {nextYear}"));
                entity.HasIndex(v => v.VIN).IsUnique();
                entity.HasIndex(v => v.LicensePlate).IsUnique();
                entity.Property(v => v.Type)
                    .HasConversion<string>();
                entity.Property(v => v.VIN)
                    .HasConversion(
                        vin => vin.Value,
                        value => new Vin(value))
                    .HasMaxLength(17)
                    .IsRequired();
                entity.Property(v => v.ApiKeyHash).IsRequired(false);
                entity.OwnsOne(v => v.CurrentLocation, gps =>
                {
                    gps.Property(g => g.Latitude).HasColumnName("LocationLatitude");
                    gps.Property(g => g.Longitude).HasColumnName("LocationLongitude");
                });
                entity.OwnsOne(v => v.Battery, battery =>
                {
                    battery.Property(b => b.Percentage).HasColumnName("BatteryPercentage");
                });
            });


            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Amount)
                    .HasPrecision(18, 2)
                    .IsRequired();
                entity.Property(t => t.Status)
                    .HasConversion<string>();
                entity.Property(t => t.Currency)
                    .HasConversion<string>();
                entity.Property(t => t.PaymentMethod)
                    .HasConversion<string>();
                entity.HasOne<Ride>()
                    .WithMany()
                    .HasForeignKey(t => t.RideId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<MaintenanceLog>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.Cost)
                    .HasPrecision(18, 2);
                entity.Property(m => m.Description)
                    .HasMaxLength(500)
                    .IsRequired();
                entity.Property(m => m.TechnicianName)
                    .HasMaxLength(100)
                    .IsRequired();
                entity.HasOne<Vehicle>()
                    .WithMany()
                    .HasForeignKey(m => m.VehicleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SupportTicket>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Status)
                    .HasConversion<string>();
                entity.Property(s => s.Priority)
                    .HasConversion<string>();
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(s => s.PassengerId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<DiscountCode>(entity =>
            {
                entity.Property(d => d.Code)
                    .HasMaxLength(50)
                    .IsRequired();
                entity.HasIndex(d => d.Code)
                    .IsUnique();
                entity.Property(d => d.Value)
                    .HasPrecision(18, 2);
                entity.Property(d => d.MinimumRideValue)
                    .HasPrecision(18, 2);
                entity.Property(d => d.Type)
                    .HasConversion<string>();
            });
        }
    }