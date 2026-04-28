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

        // GET /api/v1/discounts — admin gets all discount codes
        group.MapGet("/", async (
            IDiscountService  discountService,
            CancellationToken ct) =>
        {
            var codes = await discountService.GetAllCodes(ct);
            return Results.Ok(codes);
        })
        .RequireAuthorization("admin:discounts")
        .WithName("GetAllDiscountCodes")
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

        // DELETE /api/v1/discounts/{codeId} — admin permanently deletes code
        group.MapDelete("/{codeId:guid}", async (
            Guid              codeId,
            IDiscountService  discountService,
            CancellationToken ct) =>
        {
            await discountService.DeleteCode(codeId, ct);
            return Results.NoContent();
        })
        .RequireAuthorization("admin:discounts")
        .WithName("DeleteDiscountCode")
        .WithTags("Discounts");

        // PUT /api/v1/discounts/{codeId}/activate — admin reactivates code
        group.MapPut("/{codeId:guid}/activate", async (
            Guid              codeId,
            IDiscountService  discountService,
            CancellationToken ct) =>
        {
            await discountService.ReactivateCode(codeId, DateTimeOffset.UtcNow.AddDays(30), ct);
            return Results.NoContent();
        })
        .RequireAuthorization("admin:discounts")
        .WithName("ReactivateDiscountCode")
        .WithTags("Discounts");

        // PUT /api/v1/discounts/{codeId}/deactivate — admin deactivates code
        group.MapPut("/{codeId:guid}/deactivate", async (
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