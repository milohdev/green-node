namespace GreenNode.Application.Common.Interfaces;

public record EvidenceValidationResult(
    bool IsValid,
    decimal Confidence,
    int EstimatedTrees,
    string Observations
);

public interface IAiService
{
    Task<string> GenerateQuoteProposalAsync(string sector, int numberOfTrees, string territory);
    Task<EvidenceValidationResult> ValidateEvidencePhotoAsync(string base64Image, string mediaType);
    Task<string> GetProjectAdvisorResponseAsync(string companyName, string sector, int numberOfTrees, int validatedTrees, decimal progress, string userQuestion);
}
