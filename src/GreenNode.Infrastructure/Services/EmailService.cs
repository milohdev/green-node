using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;
using GreenNode.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GreenNode.Infrastructure.Services;

public class EmailService(
    IAmazonSimpleEmailService sesClient,
    IConfiguration configuration,
    ILogger<EmailService> logger)
    : IEmailService
{
    private string FromEmail => configuration["AWS:SesFromEmail"]
        ?? throw new InvalidOperationException("AWS:SesFromEmail no configurado.");

    private async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var request = new SendEmailRequest
        {
            Source = FromEmail,
            Destination = new Destination { ToAddresses = [toEmail] },
            Message = new Message
            {
                Subject = new Content(subject),
                Body = new Body
                {
                    Html = new Content(htmlBody)
                }
            }
        };
        await sesClient.SendEmailAsync(request);
    }

    public async Task SendQuoteReceivedEmailAsync(string toEmail, string companyName, Guid quoteId, CancellationToken ct = default)
    {
        try
        {
            var html = $"""
                <h2>¡Hola, {companyName}!</h2>
                <p>Hemos recibido tu solicitud de cotización con ID <strong>{quoteId}</strong>.</p>
                <p>Nuestro equipo la revisará pronto y recibirás una notificación con la respuesta.</p>
                <p>Gracias por contribuir a la reforestación de Colombia 🌳</p>
                <br/><p><em>Equipo GreenNode</em></p>
                """;
            await SendAsync(toEmail, "GreenNode — Cotización recibida", html);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error enviando email de cotización recibida a {Email}", toEmail);
        }
    }

    public async Task SendQuoteApprovedEmailAsync(string toEmail, string companyName, Guid projectId, CancellationToken ct = default)
    {
        try
        {
            var html = $"""
                <h2>¡Excelentes noticias, {companyName}!</h2>
                <p>Tu cotización ha sido <strong>aprobada</strong>.</p>
                <p>Se ha creado tu proyecto con ID <strong>{projectId}</strong>.</p>
                <p>Puedes consultar el avance desde tu dashboard en cualquier momento.</p>
                <br/><p><em>Equipo GreenNode</em></p>
                """;
            await SendAsync(toEmail, "GreenNode — ¡Cotización aprobada!", html);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error enviando email de cotización aprobada a {Email}", toEmail);
        }
    }

    public async Task SendQuoteRejectedEmailAsync(string toEmail, string companyName, string reason, CancellationToken ct = default)
    {
        try
        {
            var html = $"""
                <h2>Hola, {companyName}</h2>
                <p>Lamentamos informarte que tu cotización no fue aprobada.</p>
                <p><strong>Motivo:</strong> {reason}</p>
                <p>Si tienes dudas, no dudes en contactarnos.</p>
                <br/><p><em>Equipo GreenNode</em></p>
                """;
            await SendAsync(toEmail, "GreenNode — Cotización no aprobada", html);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error enviando email de cotización rechazada a {Email}", toEmail);
        }
    }

    public async Task SendCertificateReadyEmailAsync(string toEmail, string companyName, CancellationToken ct = default)
    {
        try
        {
            var html = $"""
                <h2>¡Felicitaciones, {companyName}!</h2>
                <p>Has completado tu proyecto de reforestación. ¡Increíble trabajo!</p>
                <p>Tu certificado oficial está disponible para descargar desde tu dashboard.</p>
                <br/><p><em>Equipo GreenNode</em></p>
                """;
            await SendAsync(toEmail, "GreenNode — Tu certificado está listo 🎉", html);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error enviando email de certificado listo a {Email}", toEmail);
        }
    }
}
