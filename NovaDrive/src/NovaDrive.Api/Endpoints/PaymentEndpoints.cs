namespace NovaDrive.Api.Endpoints;

public static class PaymentEndpoints
{
    public static RouteGroupBuilder MapPaymentEndpoints(this RouteGroupBuilder group)
    {
        // POST /api/v1/payments — process payment for a completed ride
        group.MapPost("/", async (
            ProcessPaymentRequest request,
            IPaymentService       paymentService,
            CancellationToken     ct) =>
        {
            var transaction = await paymentService.ProcessPayment(request, ct);
            return Results.Created($"/api/v1/payments/{transaction.RideId}", transaction);
        })
        .RequireAuthorization("create:rides")
        .WithName("ProcessPayment")
        .WithTags("Payments");

        // GET /api/v1/payments/{rideId} — get transaction for a ride
        group.MapGet("/{rideId:guid}", async (
            Guid              rideId,
            IPaymentService   paymentService,
            CancellationToken ct) =>
        {
            var transaction = await paymentService.GetTransactionByRide(rideId, ct);
            return Results.Ok(transaction);
        })
        .RequireAuthorization("read:rides")
        .WithName("GetTransactionByRide")
        .WithTags("Payments");

        return group;
    }
}