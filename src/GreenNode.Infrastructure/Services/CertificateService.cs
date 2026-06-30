using GreenNode.Application.Common.Interfaces;
using GreenNode.Application.Common.Models;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GreenNode.Infrastructure.Services;

public class CertificateService(IStorageService storageService, ILogger<CertificateService> logger)
    : ICertificateService
{
    public async Task<string> GenerateCertificateAsync(CertificateData data, CancellationToken ct = default)
    {
        try
        {
            using var stream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

                    page.Content().Column(col =>
                    {
                        // Encabezado
                        col.Item().AlignCenter().Text("CERTIFICADO DE REFORESTACIÓN")
                            .FontSize(26).Bold().FontColor("#2D6A4F");

                        col.Item().AlignCenter().PaddingTop(4).Text(
                            "GreenNode — Programa de Siembra de Árboles en Colombia")
                            .FontSize(13).FontColor("#555555");

                        col.Item().PaddingVertical(16).LineHorizontal(1).LineColor("#2D6A4F");

                        // Cuerpo
                        col.Item().PaddingTop(8).Column(body =>
                        {
                            body.Item().PaddingBottom(8).Row(row =>
                            {
                                row.RelativeItem().Text("Empresa beneficiaria:").Bold();
                                row.RelativeItem(2).Text(data.CompanyName);
                            });
                            body.Item().PaddingBottom(8).Row(row =>
                            {
                                row.RelativeItem().Text("Territorio:").Bold();
                                row.RelativeItem(2).Text(data.TerritoryName);
                            });
                            body.Item().PaddingBottom(8).Row(row =>
                            {
                                row.RelativeItem().Text("Árboles plantados y validados:").Bold();
                                row.RelativeItem(2).Text(data.NumberOfTrees.ToString("N0"));
                            });
                            body.Item().PaddingBottom(8).Row(row =>
                            {
                                row.RelativeItem().Text("CO₂ estimado capturado:").Bold();
                                row.RelativeItem(2).Text($"{data.Co2EstimatedKg:N2} kg");
                            });
                            body.Item().PaddingBottom(8).Row(row =>
                            {
                                row.RelativeItem().Text("Fecha de emisión:").Bold();
                                row.RelativeItem(2).Text(data.IssuedAt.ToString("dd/MM/yyyy"));
                            });
                            body.Item().PaddingBottom(8).Row(row =>
                            {
                                row.RelativeItem().Text("Código de verificación:").Bold();
                                row.RelativeItem(2).Text(data.VerificationCode).FontFamily("Courier New");
                            });
                        });

                        col.Item().PaddingVertical(16).LineHorizontal(1).LineColor("#2D6A4F");

                        // Pie de página
                        col.Item().AlignCenter().PaddingTop(8).Text(
                            "Este certificado valida la participación en el programa de reforestación GreenNode.")
                            .FontSize(10).FontColor("#777777").Italic();
                    });
                });
            }).GeneratePdf(stream);

            stream.Position = 0;
            var fileName = $"certificado_{data.VerificationCode}.pdf";
            return await storageService.UploadCertificateAsync(stream, fileName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generando certificado PDF para {Company}", data.CompanyName);
            throw;
        }
    }
}
