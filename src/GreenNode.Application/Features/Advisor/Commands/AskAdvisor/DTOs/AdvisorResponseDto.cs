namespace GreenNode.Application.Features.Advisor.Commands.AskAdvisor.DTOs;

public record AdvisorResponseDto(
    string Response,
    string CompanyName,
    string ProjectSector,
    decimal ProgressPercentage
);
