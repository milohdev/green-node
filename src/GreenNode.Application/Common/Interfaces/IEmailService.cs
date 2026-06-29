namespace GreenNode.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendQuoteReceivedEmailAsync(
        string toEmail, string companyName, Guid quoteId,
        CancellationToken ct = default);

    Task SendQuoteApprovedEmailAsync(
        string toEmail, string companyName, Guid projectId,
        CancellationToken ct = default);

    Task SendQuoteRejectedEmailAsync(
        string toEmail, string companyName, string reason,
        CancellationToken ct = default);

    Task SendCertificateReadyEmailAsync(
        string toEmail, string companyName,
        CancellationToken ct = default);
}
