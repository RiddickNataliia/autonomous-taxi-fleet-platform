namespace NovaDrive.Api.Endpoints;

public static class DiscountCodeEndpoints
{
    public static RouteGroupBuilder MapDiscountCodeEndpoints(this RouteGroupBuilder group)
    {
        // POST /api/v1/discounts — admin creates discount code
        group.MapPost("/", async (
            CreateDiscountCodeRequest request,
            IDiscountService          discountService,
            CancellationToken         ct) =>
        {
            var code = await discountService.CreateCode(request, ct);
            return Results.Created($"/api/v1/discounts/{code.Code}", code);
        })
        .RequireAuthorization("admin:discounts")
        .WithName("CreateDiscountCode")
        .WithTags("Discounts");

        // GET /api/v1/discounts/{code} — get discount code details
        group.MapGet("/{code}", async (
            string            code,
            IDiscountService  discountService,
            CancellationToken ct) =>
        {
            var discount = await discountService.GetByCode(code, ct);
            return Results.Ok(discount);
        })
        .RequireAuthorization("admin:discounts")
        .WithName("GetDiscountCode")
        .WithTags("Discounts");

        // DELETE /api/v1/discounts/{codeId} — admin deactivates code
        group.MapDelete("/{codeId:guid}", async (
            Guid              codeId,
            IDiscountService  discountService,
            CancellationToken ct) =>
        {
            await discountService.DeactivateCode(codeId, ct);
            return Results.NoContent();
        })
        .RequireAuthorization("admin:discounts")
        .WithName("DeactivateDiscountCode")
        .WithTags("Discounts");

        return group;
    }
}