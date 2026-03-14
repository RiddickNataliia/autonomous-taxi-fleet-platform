using NovaDrive.Domain.Entities;

namespace NovaDrive.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
        : base(options)
    {
    }

    // Register your Postgres entities here
    public DbSet<Ride> Rides { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique(); // Security: No duplicate accounts
            
            entity.Property(u => u.LoyaltyPoints)
                .HasDefaultValue(0);
        });

        modelBuilder.Entity<Ride>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.NetAmount).HasPrecision(18, 2);
            entity.Property(r => r.VatAmount).HasPrecision(18, 2);
            entity.Property(r => r.FinalPrice).HasPrecision(18, 2);
            entity.Property(r => r.LoyaltyDiscountApplied).HasPrecision(18, 2);

            // Mapping the Enum to a String in Postgres
            entity.Property(r => r.Status)
                .HasConversion<string>() 
                .IsRequired();

            // Ensuring the price has 2 decimal places in Postgres
            entity.Property(r => r.FinalPrice)
                .HasPrecision(18, 2)
                .IsRequired();
                
            // Optional: You can also configure the User foreign keys here
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(r => r.PassengerId);
        });


        modelBuilder.Entity<Vehicle>(entity =>
        {
            // Define the table name and the check constraint
            int nextYear = DateTime.UtcNow.Year + 1;
            entity.ToTable(t => t.HasCheckConstraint("CK_Vehicle_Year", $"\"YearOfManufacture\" > 1900 AND \"YearOfManufacture\" <= {nextYear}"));
            
            // Define unique indexes for security and performance
            entity.HasIndex(v => v.VIN).IsUnique();
            entity.HasIndex(v => v.LicensePlate).IsUnique();

            // Map the Enum to string for better readability in SQL
            entity.Property(v => v.Type)
                .HasConversion<string>();
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

            // Link it to the Ride
            entity.HasOne<Ride>()
                .WithMany()
                .HasForeignKey(t => t.RideId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a ride if it has transactions
        });

        modelBuilder.Entity<MaintenanceLog>(entity =>
        {
            entity.HasKey(m => m.Id);

            entity.Property(m => m.Cost)
                .HasPrecision(18, 2);

            // One Vehicle has Many MaintenanceLogs
            entity.HasOne<Vehicle>()
                .WithMany() 
                .HasForeignKey(m => m.VehicleId)
                .OnDelete(DeleteBehavior.Cascade); // If a vehicle is deleted, delete its logs
        });

        modelBuilder.Entity<SupportTicket>(entity =>
        {
            entity.HasKey(s => s.Id);

            entity.Property(s => s.Status)
                .HasConversion<string>();

            entity.Property(s => s.Priority)
                .HasConversion<string>();

            // Link to User (Passenger)
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(s => s.PassengerId);
        });

        modelBuilder.Entity<DiscountCode>(entity =>
        {
            entity.HasIndex(d => d.Code).IsUnique();

            entity.Property(d => d.Value)
                .HasPrecision(18, 2);

            entity.Property(d => d.MinimumRideValue)
                .HasPrecision(18, 2);

            entity.Property(d => d.Type)
                .HasConversion<string>();
        });
    }
}