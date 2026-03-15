namespace NovaDrive.Domain.Interfaces;

public interface ISupportTicketRepository : IRepository<SupportTicket>
{
    Task<IEnumerable<SupportTicket>> GetByPassengerId(Guid passengerId, CancellationToken cancellationToken = default);
}