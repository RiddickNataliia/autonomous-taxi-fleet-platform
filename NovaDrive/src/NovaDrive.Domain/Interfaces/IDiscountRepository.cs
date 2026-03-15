namespace NovaDrive.Domain.Interfaces;

public interface IDiscountCodeRepository : IRepository<DiscountCode>
{
    Task<DiscountCode?> GetByCode(string code, CancellationToken cancellationToken = default);
}