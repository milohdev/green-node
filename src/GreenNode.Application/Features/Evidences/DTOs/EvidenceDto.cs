namespace GreenNode.Application.Features.Evidences.DTOs;

public record EvidenceDto(
    Guid Id,
    Guid ProjectId,
    string PhotoUrl,
    bool AiValidated,
    decimal? AiConfidence,
    int? AiEstimatedTrees,
    string? AiObservations,
    DateTime CreatedAt
);
