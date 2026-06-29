using GreenNode.Domain.Common;

namespace GreenNode.Domain.Entities;

public class Evidence : BaseEntity
{
    public Guid ProjectId { get; private set; }
    public string PhotoUrl { get; private set; } = null!;
    public bool AiValidated { get; private set; }
    public decimal? AiConfidence { get; private set; }
    public int? AiEstimatedTrees { get; private set; }
    public string? AiObservations { get; private set; }

    public Project Project { get; private set; } = null!;

    public Evidence(Guid projectId, string photoUrl)
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        ProjectId = projectId;
        PhotoUrl = photoUrl;
        AiValidated = false;
    }

    protected Evidence() { }

    public void SetAiValidation(bool validated, decimal confidence,
        int estimatedTrees, string observations)
    {
        AiValidated = validated;
        AiConfidence = confidence;
        AiEstimatedTrees = estimatedTrees;
        AiObservations = observations;
    }
}
