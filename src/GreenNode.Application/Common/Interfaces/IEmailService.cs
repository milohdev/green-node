namespace GreenNode.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendQuoteReceivedEmailAsync(string toEmail, string companyName, Guid quoteId);
    Task SendQuoteApprovedEmailAsync(string toEmail, string companyName, Guid projectId);
    Task SendQuoteRejectedEmailAsync(string toEmail, string companyName, string reason);
    Task SendCertificateReadyEmailAsync(string toEmail, string companyName);
}
