namespace NovaDrive.Infrastructure.Repositories;

internal sealed class SupportTicketRepository : BaseRepository<SupportTicket>, ISupportTicketRepository
{
    public SupportTicketRepository(ApplicationDbContext context) : base(context) { }

    /// <summary>
    /// Returns all support tickets raised by a given passenger, newest first.
    /// Used on the passenger dashboard and in the admin dashboard to show a passenger's support history.
    /// </summary>
    public async Task<IEnumerable<SupportTicket>> GetByPassengerId(
        Guid passengerId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .Where(t => t.PassengerId == passengerId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);
}
