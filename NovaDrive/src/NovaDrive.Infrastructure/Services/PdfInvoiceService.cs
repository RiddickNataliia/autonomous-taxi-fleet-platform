namespace NovaDrive.Infrastructure.Services;

public sealed class PdfInvoiceService : IInvoiceService
{
    public byte[] GenerateInvoice(
        string passengerName, string passengerEmail,
        string departure, string destination,
        double distanceKm, int durationMinutes,
        decimal netAmount, decimal vatAmount, decimal totalAmount,
        decimal loyaltyDiscount, decimal codeDiscount,
        string paymentMethod, string paymentStatus,
        string? bankReference, DateTimeOffset paymentDate)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Text("NOVA DRIVE")
                            .FontSize(24).Bold().FontColor("#1a1a2e");
                        row.ConstantItem(120).AlignRight().Text($"INVOICE")
                            .FontSize(18).Bold().FontColor("#4a4e69");
                    });
                    col.Item().PaddingTop(4).Text("Autonomous Mobility as a Service")
                        .FontSize(10).FontColor("#6c757d");
                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor("#dee2e6");
                });

                page.Content().PaddingTop(20).Column(col =>
                {
                    // Passenger info
                    col.Item().Text($"Passenger: {passengerName}").Bold();
                    col.Item().Text($"Email: {passengerEmail}").FontColor("#6c757d");
                    col.Item().PaddingTop(4).Text($"Date: {paymentDate:dd MMM yyyy HH:mm}").FontColor("#6c757d");

                    col.Item().PaddingTop(16).Text("Ride Details").FontSize(13).Bold();
                    col.Item().PaddingTop(4).LineHorizontal(1).LineColor("#dee2e6");

                    col.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(2);
                        });

                        table.Cell().Text("From").Bold();
                        table.Cell().Text(departure);
                        table.Cell().Text("To").Bold();
                        table.Cell().Text(destination);
                        table.Cell().Text("Distance").Bold();
                        table.Cell().Text($"{distanceKm:F1} km");
                        table.Cell().Text("Duration").Bold();
                        table.Cell().Text($"{durationMinutes} minutes");
                    });

                    col.Item().PaddingTop(16).Text("Pricing Breakdown").FontSize(13).Bold();
                    col.Item().PaddingTop(4).LineHorizontal(1).LineColor("#dee2e6");

                    col.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(2);
                        });

                        table.Cell().Text("Net amount").Bold();
                        table.Cell().AlignRight().Text($"€{netAmount:F2}");

                        if (loyaltyDiscount > 0)
                        {
                            table.Cell().Text("Loyalty discount").FontColor("#28a745");
                            table.Cell().AlignRight().Text($"-€{loyaltyDiscount:F2}").FontColor("#28a745");
                        }

                        if (codeDiscount > 0)
                        {
                            table.Cell().Text("Promo discount").FontColor("#28a745");
                            table.Cell().AlignRight().Text($"-€{codeDiscount:F2}").FontColor("#28a745");
                        }

                        table.Cell().Text("VAT (21%)").Bold();
                        table.Cell().AlignRight().Text($"€{vatAmount:F2}");

                        table.Cell().PaddingTop(8).Text("Total charged").Bold().FontSize(13);
                        table.Cell().PaddingTop(8).AlignRight()
                            .Text($"€{totalAmount:F2}").Bold().FontSize(13).FontColor("#1a1a2e");
                    });

                    col.Item().PaddingTop(16).Text("Payment").FontSize(13).Bold();
                    col.Item().PaddingTop(4).LineHorizontal(1).LineColor("#dee2e6");

                    col.Item().PaddingTop(8).Table(table =>
                    {
                        table.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(3);
                            cols.RelativeColumn(2);
                        });

                        table.Cell().Text("Method").Bold();
                        table.Cell().Text(paymentMethod);
                        table.Cell().Text("Status").Bold();
                        table.Cell().Text(paymentStatus).FontColor("#28a745");
                        table.Cell().Text("Reference").Bold();
                        table.Cell().Text(bankReference ?? "-");
                    });
                });

                page.Footer().AlignCenter()
                    .Text($"Thank you for riding with Nova Drive — {bankReference ?? "N/A"}")
                    .FontSize(9).FontColor("#adb5bd");
            });
        }).GeneratePdf();
    }
}