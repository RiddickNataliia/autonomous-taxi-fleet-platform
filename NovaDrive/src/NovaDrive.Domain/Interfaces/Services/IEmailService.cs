namespace NovaDrive.Domain.Interfaces;

public interface IEmailService
{
    Task SendInvoice(string toEmail, string passengerName, byte[] pdfBytes, CancellationToken ct = default);
}