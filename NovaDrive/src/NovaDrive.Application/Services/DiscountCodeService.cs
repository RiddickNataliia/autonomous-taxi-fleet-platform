// // In DiscountCodeService (Application layer)
// public async Task CreateDiscountCode(CreateDiscountCodeDto dto)
// {
//     var code = DiscountCode.Create(
//         dto.Code,
//         dto.Type,
//         dto.Value,
//         dto.MinimumRideValue,
//         dto.ExpirationDate);

//     await _discountRepository.Add(code);
//     await _unitOfWork.SaveChanges();
// }