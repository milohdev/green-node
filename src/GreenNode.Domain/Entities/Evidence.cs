namespace GreenNode.Domain.Entities;

public class Evidence
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public string PhotoUrl { get; private set; } = default!;
    public bool AiValidated { get; private set; }
    public decimal? AiConfidence { get; private set; }
    public int? AiEstimatedTrees { get; private set; }
    public string? AiObservations { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Project Project { get; private set; } = default!;

    public Evidence(Guid projectId, string photoUrl)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        PhotoUrl = photoUrl;
        AiValidated = false;
        CreatedAt = DateTime.UtcNow;
    }

    protected Evidence() { }

    public void SetAiValidation(bool validated, decimal confidence, int estimatedTrees, string observations)
    {
        AiValidated = validated;
        AiConfidence = confidence;
        AiEstimatedTrees = estimatedTrees;
        AiObservations = observations;
    }
}
