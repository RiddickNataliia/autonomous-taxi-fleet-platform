namespace NovaDrive.Domain.Interfaces;

public interface IVehicleRepository : IRepository<Vehicle>
{
    Task<IEnumerable<Vehicle>> GetAllActive(CancellationToken cancellationToken = default);
    Task<Vehicle?> GetByVin(string vin, CancellationToken cancellationToken = default);
}