namespace NovaDrive.Api.Endpoints;

public static class SupportTicketEndpoints
{
    public static RouteGroupBuilder MapSupportTicketEndpoints(this RouteGroupBuilder group)
    {
        // POST /api/v1/support — passenger creates ticket
        group.MapPost("/", async (
            HttpContext              context,
            CreateTicketRequest      request,
            ISupportService          supportService,
            IUserProvisioningService provisioning,
            CancellationToken        ct) =>
        {
            var auth0UserId = context.User.Auth0UserId()
                ?? throw new UnauthorizedAccessException("Missing subject claim.");
            var email = context.User.Email()
                ?? throw new UnauthorizedAccessException("Missing email claim.");

            var (passenger, _) = await provisioning.EnsurePassengerExists(auth0UserId, email, ct);
            var requestWithPassenger = request with { PassengerId = passenger.PassengerId };
            var ticket = await supportService.CreateTicket(requestWithPassenger, ct);
            return Results.Created($"/api/v1/support/{ticket.TicketId}", ticket);
        })
        .RequireAuthorization("read:profile")
        .WithName("CreateTicket")
        .WithTags("Support");

        // GET /api/v1/support/my — passenger gets own tickets
        group.MapGet("/my", async (
            HttpContext              context,
            ISupportService          supportService,
            IUserProvisioningService provisioning,
            CancellationToken        ct) =>
        {
            var auth0UserId = context.User.Auth0UserId()
                ?? throw new UnauthorizedAccessException("Missing subject claim.");
            var email = context.User.Email()
                ?? throw new UnauthorizedAccessException("Missing email claim.");

            var (passenger, _) = await provisioning.EnsurePassengerExists(auth0UserId, email, ct);
            var tickets = await supportService.GetTicketsByPassenger(passenger.PassengerId, ct);
            return Results.Ok(tickets);
        })
        .RequireAuthorization("read:profile")
        .WithName("GetMyTickets")
        .WithTags("Support");

        // GET /api/v1/support/{ticketId} — admin gets specific ticket
        group.MapGet("/{ticketId:guid}", async (
            Guid              ticketId,
            ISupportService   supportService,
            CancellationToken ct) =>
        {
            var ticket = await supportService.GetTicket(ticketId, ct);
            return Results.Ok(ticket);
        })
        .RequireAuthorization("admin:support")
        .WithName("GetTicket")
        .WithTags("Support");

        // PUT /api/v1/support/{ticketId}/start — admin starts work
        group.MapPut("/{ticketId:guid}/start", async (
            Guid              ticketId,
            ISupportService   supportService,
            CancellationToken ct) =>
        {
            var ticket = await supportService.StartWork(ticketId, ct);
            return Results.Ok(ticket);
        })
        .RequireAuthorization("admin:support")
        .WithName("StartWork")
        .WithTags("Support");

        // PUT /api/v1/support/{ticketId}/resolve — admin resolves ticket
        group.MapPut("/{ticketId:guid}/resolve", async (
            Guid              ticketId,
            ISupportService   supportService,
            CancellationToken ct) =>
        {
            var ticket = await supportService.ResolveTicket(ticketId, ct);
            return Results.Ok(ticket);
        })
        .RequireAuthorization("admin:support")
        .WithName("ResolveTicket")
        .WithTags("Support");

        // PUT /api/v1/support/{ticketId}/priority — admin updates priority
        group.MapPut("/{ticketId:guid}/priority", async (
            Guid                    ticketId,
            UpdatePriorityRequest   request,
            ISupportService         supportService,
            CancellationToken       ct) =>
        {
            var ticket = await supportService.UpdatePriority(ticketId, request.Priority, ct);
            return Results.Ok(ticket);
        })
        .RequireAuthorization("admin:support")
        .WithName("UpdatePriority")
        .WithTags("Support");

        return group;
    }
}