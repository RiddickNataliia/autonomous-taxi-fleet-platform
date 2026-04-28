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


    public static SupportTicket Create(Guid passengerId, string subject, string description)
    {
        if (string.IsNullOrWhiteSpace(subject)) 
            throw new SupportDomainException(SupportDomainException.InvalidSubject);

        if (string.IsNullOrWhiteSpace(description)) 
            throw new SupportDomainException(SupportDomainException.InvalidDescription);
        
        return new SupportTicket
        {
            PassengerId = passengerId,
            Subject     = subject,
            Description = description
        };
    }


    public void StartWork()
    {
        if (Status != TicketStatus.Open)
            throw new SupportDomainException(SupportDomainException.NotOpen);

        Status = TicketStatus.InProgress;
    }


    public void Resolve()
    {

        if (Status != TicketStatus.InProgress)
            throw new SupportDomainException(SupportDomainException.NotInProgress);

        Status = TicketStatus.Resolved;
        ResolvedAt = DateTimeOffset.UtcNow;
    }


    public void UpdatePriority(TicketPriority newPriority)
    {
        if (newPriority == TicketPriority.Unknown)
            throw new SupportDomainException(SupportDomainException.InvalidPriority);

        if (newPriority == Priority) return;

        Priority = newPriority;
    }
}