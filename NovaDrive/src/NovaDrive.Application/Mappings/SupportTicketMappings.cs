namespace NovaDrive.Application.Mappings;

public static class SupportTicketMappings
{
    public static TicketResponse MapToResponse(this SupportTicket t) => new(
        TicketId:    t.Id,
        PassengerId: t.PassengerId,
        Subject:     t.Subject,
        Description: t.Description,
        Priority:    t.Priority.ToString(),
        Status:      t.Status.ToString(),
        CreatedAt:   t.CreatedAt,
        ResolvedAt:  t.ResolvedAt);
}