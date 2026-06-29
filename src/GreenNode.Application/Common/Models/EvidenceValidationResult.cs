namespace GreenNode.Application.Common.Models;

public record EvidenceValidationResult(
    bool IsValid,
    decimal Confidence,
    int EstimatedTrees,
    string Observations
);
