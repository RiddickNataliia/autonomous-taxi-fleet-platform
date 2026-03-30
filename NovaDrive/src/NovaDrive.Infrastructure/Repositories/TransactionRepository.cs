namespace NovaDrive.Infrastructure.Repositories;

internal sealed class TransactionRepository : BaseRepository<Transaction>, ITransactionRepository
{
    public TransactionRepository(ApplicationDbContext context) : base(context) { }

    /// <summary>
    /// Returns the financial transaction linked to a completed ride.
    /// </summary>
    public async Task<Transaction?> GetByRideId(
        Guid rideId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .SingleOrDefaultAsync(t => t.RideId == rideId, cancellationToken); // A ride should only ever have one transaction (the ON DELETE RESTRICT
                                                                               // constraint in the DB enforces this), so SingleOrDefault is correct here.
}
