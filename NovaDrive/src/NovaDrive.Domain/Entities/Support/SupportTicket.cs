namespace NovaDrive.Domain.Entities;

public class SupportTicket
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Guid PassengerId { get; init; }
    public string Subject { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TicketPriority Priority { get; private set; } = TicketPriority.Unknown;
    public TicketStatus Status { get; private set; } = TicketStatus.Open;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ResolvedAt { get; private set; }

    // Private constructor for EF Core
    private SupportTicket() { }

    /// <summary>
    /// Factory method to create a new ticket in the 'Open' state.
    /// </summary>
    public static SupportTicket Create(Guid passengerId, string subject, string description, TicketPriority priority)
    {
        if (string.IsNullOrWhiteSpace(subject)) throw new SupportDomainException(SupportDomainException.InvalidSubject);
        
        return new SupportTicket
        {
            PassengerId = passengerId,
            Subject = subject,
            Description = description,
            Priority = priority,
            Status = TicketStatus.Open
        };
    }

    /// <summary>
    /// Moves the ticket to 'In Progress'.
    /// </summary>
    public void StartWork()
    {
        if (Status != TicketStatus.Open)
            throw new SupportDomainException(SupportDomainException.NotOpen);

        Status = TicketStatus.InProgress;
    }

    /// <summary>
    /// Resolves the ticket and records the timestamp.
    /// </summary>
    public void Resolve()
    {
        if (Status == TicketStatus.Resolved) return;

        Status = TicketStatus.Resolved;
        ResolvedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Allows changing priority if the issue is more urgent than initially reported.
    /// </summary>
    public void UpdatePriority(TicketPriority newPriority)
    {
        Priority = newPriority;
    }
}