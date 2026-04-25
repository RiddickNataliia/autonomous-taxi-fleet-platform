using NovaDrive.Application.DTOs;
using NovaDrive.Domain.Exceptions;
using NovaDrive.Domain.Interfaces;

namespace NovaDrive.Application.Services;

public interface IDiscountService
{
    Task<DiscountCodeResponse> CreateCode(CreateDiscountCodeRequest request, CancellationToken ct = default);
    Task<DiscountCodeResponse> GetByCode(string code, CancellationToken ct = default);
    Task DeactivateCode(Guid codeId, CancellationToken ct = default);
}
public sealed class DiscountService : IDiscountService
{
    private readonly IDiscountCodeRepository _discountRepo;
    private readonly IUnitOfWork             _unitOfWork;

    public DiscountService(IDiscountCodeRepository discountRepo, IUnitOfWork unitOfWork)
    {
        _discountRepo = discountRepo;
        _unitOfWork   = unitOfWork;
    }

    public async Task<DiscountCodeResponse> CreateCode(
        CreateDiscountCodeRequest request, CancellationToken ct = default)
    {
        var existing = await _discountRepo.GetByCode(request.Code, ct);
        if (existing is not null)
            throw new DiscountDomainException(DiscountDomainException.CodeAlreadyExists);
            
        if (!Enum.TryParse<DiscountType>(request.Type, ignoreCase: true, out var type)
            || type == DiscountType.Unknown)
            throw new DiscountDomainException(DiscountDomainException.InvalidType);

        var code = DiscountCode.Create(
            request.Code,
            type,
            request.Value,
            request.MinimumRideValue,
            request.ExpirationDate);

        await _discountRepo.Add(code, ct);
        await _unitOfWork.SaveChanges(ct);
        return code.ToResponse();
    }

    public async Task<DiscountCodeResponse> GetByCode(string code, CancellationToken ct = default)
    {
        var entity = await _discountRepo.GetByCode(code, ct)
            ?? throw new KeyNotFoundException($"Discount code '{code}' not found.");
        return entity.ToResponse();
    }

    public async Task DeactivateCode(Guid codeId, CancellationToken ct = default)
    {
        var entity = await _discountRepo.GetById(codeId, ct)
            ?? throw new KeyNotFoundException("Discount code not found.");

        entity.Deactivate();
        await _unitOfWork.SaveChanges(ct);
    }

}
