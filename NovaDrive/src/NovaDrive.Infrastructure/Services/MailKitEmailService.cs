namespace NovaDrive.Infrastructure.Services;

public sealed class MailKitEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<MailKitEmailService> _logger;

    public MailKitEmailService(IConfiguration config, ILogger<MailKitEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendInvoice(
        string toEmail, string passengerName, byte[] pdfBytes, CancellationToken ct = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            _config["Email:FromName"] ?? "NovaDrive",
            _config["Email:FromAddress"] ?? "noreply@novadrive.com"));
        message.To.Add(new MailboxAddress(passengerName, toEmail));
        message.Subject = "Your NovaDrive Invoice";

        var builder = new BodyBuilder
        {
            TextBody = $"Dear {passengerName},\n\nThank you for riding with Nova Drive.\nPlease find your invoice attached.\n\nNova Drive Team"
        };
        builder.Attachments.Add("invoice.pdf", pdfBytes, ContentType.Parse("application/pdf"));
        message.Body = builder.ToMessageBody();

        using var client = new MailKit.Net.Smtp.SmtpClient();
        await client.ConnectAsync(
            _config["Email:SmtpHost"] ?? "localhost",
            int.Parse(_config["Email:SmtpPort"] ?? "1025"),
            MailKit.Security.SecureSocketOptions.None, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);

        _logger.LogInformation("Invoice sent to {Email}", toEmail);
    }
}