namespace NovaDrive.Infrastructure.Repositories;

internal sealed class DiscountCodeRepository : BaseRepository<DiscountCode>, IDiscountCodeRepository
{
    public DiscountCodeRepository(ApplicationDbContext context) : base(context) { }

    /// <summary>
    /// Looks up a discount code by its humanreadable string.
    /// Comparison is case-insensitive.
    /// Used during ride booking to validate a discount code and apply the discount.
    /// </summary>
    public async Task<DiscountCode?> GetByCode(
        string code,
        CancellationToken cancellationToken = default)
        => await DbSet
            .FirstOrDefaultAsync(
                d => d.Code.ToLower() == code.ToLower(),
                cancellationToken);

}
