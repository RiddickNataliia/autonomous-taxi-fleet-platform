namespace NovaDrive.Domain.Interfaces;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task<Transaction?> GetByRideId(Guid rideId, CancellationToken cancellationToken = default);
}