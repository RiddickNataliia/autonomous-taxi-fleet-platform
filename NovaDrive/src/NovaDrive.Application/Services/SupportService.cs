namespace NovaDrive.Application.Services;

public interface ISupportService
{
    Task<TicketResponse> CreateTicket(CreateTicketRequest request, CancellationToken ct = default);
    Task<TicketResponse> GetTicket(Guid ticketId, CancellationToken ct = default);
    Task<IEnumerable<TicketResponse>> GetTicketsByPassenger(Guid passengerId, CancellationToken ct = default);
    Task<TicketResponse> StartWork(Guid ticketId, CancellationToken ct = default);
    Task<TicketResponse> ResolveTicket(Guid ticketId, CancellationToken ct = default);
    Task<TicketResponse> UpdatePriority(Guid ticketId, string newPriority, CancellationToken ct = default);
}
public sealed class SupportService : ISupportService
{
    private readonly ISupportTicketRepository _ticketRepo;
    private readonly IUnitOfWork              _unitOfWork;

    public SupportService(ISupportTicketRepository ticketRepo, IUnitOfWork unitOfWork)
    {
        _ticketRepo  = ticketRepo;
        _unitOfWork  = unitOfWork;
    }

    public async Task<TicketResponse> CreateTicket(
        CreateTicketRequest request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<TicketPriority>(request.Priority, ignoreCase: true, out var priority)
            || priority == TicketPriority.Unknown)
            throw new SupportDomainException(SupportDomainException.InvalidPriority);

        var ticket = SupportTicket.Create(
            request.PassengerId,
            request.Subject,
            request.Description,
            priority);

        await _ticketRepo.Add(ticket, ct);
        await _unitOfWork.SaveChanges(ct);
        return ticket.ToResponse();
    }

    public async Task<TicketResponse> GetTicket(Guid ticketId, CancellationToken ct = default)
    {
        var ticket = await _ticketRepo.GetById(ticketId, ct)
            ?? throw new KeyNotFoundException("Support ticket not found.");
        return ticket.ToResponse();
    }

    public async Task<IEnumerable<TicketResponse>> GetTicketsByPassenger(
        Guid passengerId, CancellationToken ct = default)
        => (await _ticketRepo.GetByPassengerId(passengerId, ct)).Select(t => t.ToResponse());

    public async Task<TicketResponse> StartWork(Guid ticketId, CancellationToken ct = default)
    {
        var ticket = await _ticketRepo.GetById(ticketId, ct)
            ?? throw new KeyNotFoundException("Support ticket not found.");

        ticket.StartWork();
        await _unitOfWork.SaveChanges(ct);
        return ticket.ToResponse();
    }

    public async Task<TicketResponse> ResolveTicket(Guid ticketId, CancellationToken ct = default)
    {
        var ticket = await _ticketRepo.GetById(ticketId, ct)
            ?? throw new KeyNotFoundException("Support ticket not found.");

        ticket.Resolve();
        await _unitOfWork.SaveChanges(ct);
        return ticket.ToResponse();
    }

    public async Task<TicketResponse> UpdatePriority(
        Guid ticketId, string newPriority, CancellationToken ct = default)
    {
        if (!Enum.TryParse<TicketPriority>(newPriority, ignoreCase: true, out var priority)
            || priority == TicketPriority.Unknown)
            throw new SupportDomainException(SupportDomainException.InvalidPriority);

        var ticket = await _ticketRepo.GetById(ticketId, ct)
            ?? throw new KeyNotFoundException("Support ticket not found.");

        ticket.UpdatePriority(priority);
        await _unitOfWork.SaveChanges(ct);
        return ticket.ToResponse();
    }

}
