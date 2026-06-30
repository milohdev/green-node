namespace GreenNode.WebAPI.Contracts.Projects;

public record UpdateProgressRequest(decimal ProgressPercent, int ValidatedTreeCount);
