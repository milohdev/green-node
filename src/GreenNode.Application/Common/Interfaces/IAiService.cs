using GreenNode.Application.Common.Models;

namespace GreenNode.Application.Common.Interfaces;

public interface IClaudeAiService
{
    Task<string> GenerateQuoteProposalAsync(
        string sector, int numberOfTrees, string territory,
        CancellationToken ct = default);

    Task<EvidenceValidationResult> ValidateEvidencePhotoAsync(
        string base64Image, string mediaType,
        CancellationToken ct = default);

    Task<string> GetProjectAdvisorResponseAsync(
        string companyName, string sector, int numberOfTrees,
        int validatedTrees, decimal progress, string userQuestion,
        CancellationToken ct = default);
}
