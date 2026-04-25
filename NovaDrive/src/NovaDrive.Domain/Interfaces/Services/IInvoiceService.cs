namespace NovaDrive.Domain.Interfaces;

public interface IInvoiceService
{
    byte[] GenerateInvoice(string passengerName, string passengerEmail, 
        string departure, string destination, 
        double distanceKm, int durationMinutes,
        decimal netAmount, decimal vatAmount, decimal totalAmount,
        decimal loyaltyDiscount, decimal codeDiscount,
        string paymentMethod, string paymentStatus, 
        string? bankReference, DateTimeOffset paymentDate);
}