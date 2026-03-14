namespace NovaDrive.Domain.Entities;

public class MaintenanceLog
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid VehicleId { get; init; }
    public DateTimeOffset ServiceDate { get; init; } = DateTimeOffset.UtcNow;
    public required string Description { get; init; }
    public required string TechnicianName { get; init; }
    public decimal Cost { get; init; }
    public int? NextServiceMileage { get; init; } // Optional: Only used if the service was a mileage-based check

    /// <summary>
    /// Static factory method to ensure a log is created with a non-negative cost.
    /// </summary>
    public static MaintenanceLog Create(Guid vehicleId, string description, string technician, decimal cost)
    {
        if (cost < 0) 
            throw new MaintenanceDomainException("Maintenance cost cannot be negative.");

        return new MaintenanceLog
        {
            VehicleId = vehicleId,
            Description = description,
            TechnicianName = technician,
            Cost = Math.Round(cost, 2)
        };
    }
}