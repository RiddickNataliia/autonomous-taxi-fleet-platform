namespace NovaDrive.Domain.Interfaces;

public interface IPassengerRepository : IRepository<Passenger>
{
    Task<Passenger?> GetByUserId(Guid userId, CancellationToken cancellationToken = default);
}