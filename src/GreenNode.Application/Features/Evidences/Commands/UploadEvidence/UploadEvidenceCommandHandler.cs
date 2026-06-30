using GreenNode.Application.Common.Exceptions;
using GreenNode.Application.Common.Interfaces;
using GreenNode.Application.Common.Models;
using GreenNode.Application.Features.Evidences.DTOs;
using GreenNode.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenNode.Application.Features.Evidences.Commands.UploadEvidence;

public class UploadEvidenceCommandHandler : IRequestHandler<UploadEvidenceCommand, EvidenceDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IStorageService _storageService;
    private readonly IClaudeAiService _aiService;
    private readonly ILogger<UploadEvidenceCommandHandler> _logger;

    public UploadEvidenceCommandHandler(
        IApplicationDbContext context,
        IStorageService storageService,
        IClaudeAiService aiService,
        ILogger<UploadEvidenceCommandHandler> logger)
    {
        _context = context;
        _storageService = storageService;
        _aiService = aiService;
        _logger = logger;
    }

    public async Task<EvidenceDto> Handle(UploadEvidenceCommand request, CancellationToken ct)
    {
        Project project = await _context.Projects
            .FirstOrDefaultAsync(p => p.Id == request.ProjectId, ct)
            ?? throw new NotFoundException(nameof(Project), request.ProjectId);

        // Solo se valida pertenencia si el caller es una Company.
        // Admin (CallerCompanyId == null) puede subir evidencia a cualquier proyecto.
        if (request.CallerCompanyId is not null && project.CompanyId != request.CallerCompanyId)
            throw new ForbiddenException("No tienes acceso a este proyecto.");

        string photoKey = await _storageService.UploadEvidenceAsync(
            request.FileStream, request.FileName, request.ContentType, ct);

        Evidence evidence = new(request.ProjectId, photoKey);
        _context.Evidences.Add(evidence);
        await _context.SaveChangesAsync(ct);

        Guid evidenceId = evidence.Id;
        _ = Task.Run(async () =>
        {
            try
            {
                EvidenceValidationResult result = await _aiService.ValidateEvidencePhotoAsync(
                    photoKey, request.ContentType, CancellationToken.None);

                Evidence? evidenceToUpdate = await _context.Evidences
                    .FindAsync(evidenceId);

                if (evidenceToUpdate is not null)
                {
                    evidenceToUpdate.SetAiValidation(
                        result.IsValid, result.Confidence,
                        result.EstimatedTrees, result.Observations);

                    if (result.IsValid && result.EstimatedTrees > 0)
                        project.UpdateProgress(result.EstimatedTrees);

                    await _context.SaveChangesAsync(CancellationToken.None);

                    _logger.LogInformation(
                        "✅ Evidencia {EvidenceId} validada por IA: {IsValid}, {Trees} árboles",
                        evidenceId, result.IsValid, result.EstimatedTrees);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error en validación IA para evidencia {EvidenceId}", evidenceId);
            }
        }, CancellationToken.None);

        return new EvidenceDto(
            evidence.Id, evidence.ProjectId, evidence.PhotoUrl,
            evidence.AiValidated, evidence.AiConfidence,
            evidence.AiEstimatedTrees, evidence.AiObservations,
            evidence.CreatedAt);
    }
}
