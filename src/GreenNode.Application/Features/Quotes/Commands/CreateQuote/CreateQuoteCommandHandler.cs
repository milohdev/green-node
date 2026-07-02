using GreenNode.Application.Common.Exceptions;
using GreenNode.Application.Common.Interfaces;
using GreenNode.Application.Features.Quotes.DTOs;
using GreenNode.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GreenNode.Application.Features.Quotes.Commands.CreateQuote;

public class CreateQuoteCommandHandler : IRequestHandler<CreateQuoteCommand, QuoteDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IClaudeAiService _aiService;
    private readonly IEmailService _emailService;
    private readonly ILogger<CreateQuoteCommandHandler> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public CreateQuoteCommandHandler(
        IApplicationDbContext context,
        IClaudeAiService aiService,
        IEmailService emailService,
        ILogger<CreateQuoteCommandHandler> logger,
        IServiceScopeFactory scopeFactory)
    {
        _context = context;
        _aiService = aiService;
        _emailService = emailService;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    public async Task<QuoteDetailDto> Handle(CreateQuoteCommand request, CancellationToken ct)
    {
        Company company = await _context.Companies
            .FirstOrDefaultAsync(c => c.Id == request.CompanyId, ct)
            ?? throw new NotFoundException(nameof(Company), request.CompanyId);

        Territory territory = await _context.Territories
            .FirstOrDefaultAsync(t => t.Id == request.TerritoryId, ct)
            ?? throw new NotFoundException(nameof(Territory), request.TerritoryId);

        decimal totalPrice = territory.PricePerTree * request.NumberOfTrees;

        Quote quote = new(request.CompanyId, request.TerritoryId,
            request.Sector, request.NumberOfTrees, totalPrice);
        _context.Quotes.Add(quote);
        await _context.SaveChangesAsync(ct);

        Guid quoteId = quote.Id;
        string capturedSector = request.Sector;
        int capturedNumberOfTrees = request.NumberOfTrees;
        string capturedTerritoryName = territory.Name;

        _ = Task.Run(async () =>
        {
            try
            {
                using IServiceScope scope = _scopeFactory.CreateScope();

                IClaudeAiService aiService = scope.ServiceProvider
                    .GetRequiredService<IClaudeAiService>();
                IApplicationDbContext context = scope.ServiceProvider
                    .GetRequiredService<IApplicationDbContext>();

                string proposal = await aiService.GenerateQuoteProposalAsync(
                    capturedSector, capturedNumberOfTrees, capturedTerritoryName);

                Quote? quoteToUpdate = await context.Quotes.FindAsync(quoteId);

                if (quoteToUpdate is not null)
                {
                    quoteToUpdate.SetAiProposal(proposal);
                    await context.SaveChangesAsync(CancellationToken.None);

                    _logger.LogInformation(
                        "✅ Propuesta IA generada para Quote {QuoteId}", quoteId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error generando propuesta IA para Quote {QuoteId}", quoteId);
            }
        }, CancellationToken.None);

        _ = _emailService.SendQuoteReceivedEmailAsync(
            company.Email, company.Name, quote.Id, CancellationToken.None);

        return new QuoteDetailDto(
            quote.Id, quote.CompanyId, company.Name,
            territory.Id, territory.Name, territory.Department,
            territory.Municipality, quote.Sector, quote.NumberOfTrees,
            quote.TotalPrice, quote.Status.ToString(),
            quote.AiProposal, quote.RejectionReason,
            ProjectId: null,
            quote.CreatedAt);
    }
}
