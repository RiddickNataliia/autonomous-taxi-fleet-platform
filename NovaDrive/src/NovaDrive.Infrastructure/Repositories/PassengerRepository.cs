namespace NovaDrive.Infrastructure.Repositories;

internal sealed class PassengerRepository : BaseRepository<Passenger>, IPassengerRepository
{
    public PassengerRepository(ApplicationDbContext context) : base(context) { }

    /// <summary>
    /// Returns the passenger profile for a given user account.
    /// </summary>
    public async Task<Passenger?> GetByUserId(Guid userId, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);


    /// <summary>
    /// Returns the passenger profile along with the associated user account details, based on the passenger's unique identifier.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Passenger?> GetByIdWithUser(Guid id, CancellationToken cancellationToken = default)
    => await DbSet
        .Include(p => p.User)
        .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
}
