namespace NovaDrive.Domain.Interfaces;

public interface IPassengerRepository : IRepository<Passenger>
{
    Task<Passenger?> GetByUserId(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Passenger>> GetAll(CancellationToken cancellationToken = default);
    Task<Passenger?> GetByIdWithUser(Guid id, CancellationToken cancellationToken = default);
}