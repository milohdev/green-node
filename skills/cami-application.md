# Skill — Cami · Application

## Tu rol en GreenNode

Eres el cerebro del negocio. Decides qué pasa cuando algo pasa.
Tú defines las reglas, las validaciones y los flujos. No sabes nada 
de cómo se guarda la data ni cómo llega la petición HTTP — eso es 
problema de Vale y Juan. Tú solo sabes qué debe pasar y en qué orden.

## Lo que tocas — y solo tú tocas

- src/GreenNode.Application/ completo
- Interfaces en Common/Interfaces/ (el contrato que Vale debe cumplir)
- Features/ con toda la estructura CQRS
- Behaviors/ y Exceptions/

## Lo que NUNCA tocas

- src/GreenNode.Infrastructure/ — Vale implementa las interfaces
- src/GreenNode.WebAPI/ — Juan conecta los endpoints
- src/GreenNode.Domain/ — cambios solo con aprobación del equipo

## CQRS — la estructura que siempre debes seguir

### Commands (mutan estado)
```csharp
// 1. El request — siempre record, siempre IRequest<T>
public record ApproveQuoteCommand(Guid QuoteId) : IRequest<ProjectDto>;

// 2. El handler — toda la lógica aquí
public class ApproveQuoteCommandHandler 
    : IRequestHandler<ApproveQuoteCommand, ProjectDto>
{
    private readonly IApplicationDbContext _context;
    // inyecta interfaces de Application, nunca clases de Infrastructure

    public async Task<ProjectDto> Handle(
        ApproveQuoteCommand request, CancellationToken ct)
    {
        // lógica aquí
    }
}

// 3. El validator — FluentValidation, solo si tiene reglas
public class ApproveQuoteCommandValidator 
    : AbstractValidator<ApproveQuoteCommand>
{
    public ApproveQuoteCommandValidator()
    {
        RuleFor(x => x.QuoteId).NotEmpty();
    }
}
```

### Queries (solo leen, nunca mutan)
```csharp
// 1. El request
public record GetQuoteByIdQuery(Guid QuoteId) : IRequest<QuoteDetailDto>;

// 2. El handler — mapea entidad → DTO a mano, sin AutoMapper
public class GetQuoteByIdQueryHandler 
    : IRequestHandler<GetQuoteByIdQuery, QuoteDetailDto>
{
    public async Task<QuoteDetailDto> Handle(
        GetQuoteByIdQuery request, CancellationToken ct)
    {
        var quote = await _context.Quotes
            .Include(q => q.Territory)
            .Include(q => q.Company)
            .FirstOrDefaultAsync(q => q.Id == request.QuoteId, ct)
            ?? throw new NotFoundException(nameof(Quote), request.QuoteId);

        return new QuoteDetailDto(
            quote.Id, quote.Sector, quote.TreeCount,
            quote.Status.ToString(), quote.AiProposal,
            quote.Territory.Name, quote.RequestedAt);
    }
}
```

### Regla de oro de los Handlers
- Solo inyectas interfaces de Application (IApplicationDbContext, IAiService, etc.)
- NUNCA inyectas clases concretas de Infrastructure
- NUNCA pones lógica de negocio en un QueryHandler
- NUNCA retornas entidades de Domain — siempre DTOs

## Las reglas de negocio que NUNCA debes romper

### Regla 1 — Aprobar cotización: todo o nada
```csharp
// En ApproveQuoteCommandHandler — ambas operaciones en una transacción
var quote = await _context.Quotes.FindAsync(request.QuoteId, ct)
    ?? throw new NotFoundException(nameof(Quote), request.QuoteId);

quote.Approve(); // método del domain, no setter directo

var project = new Project(quote.Id, quote.CompanyId);
_context.Projects.Add(project);

await _context.SaveChangesAsync(ct); // un solo SaveChanges = una transacción
// Si esto falla, ni Quote ni Project quedan a medias
```

### Regla 2 — Seguridad por empresa
```csharp
// En CUALQUIER query de empresa, filtrar SIEMPRE por companyId del JWT
// El companyId viene del Command/Query, que lo recibe del Controller
// que lo extrae del JWT — nunca de la URL

var projects = await _context.Projects
    .Where(p => p.CompanyId == request.CompanyId) // obligatorio
    .ToListAsync(ct);

// Si un admin pide un proyecto específico y el Id no existe → NotFoundException
// Si una empresa pide un proyecto que no es suyo → ForbiddenException
```

### Regla 3 — Progreso al 100% completa el proyecto
```csharp
// Esto pasa en el domain method, NO en el handler
// En Project.cs (Domain):
public void UpdateProgress(decimal progressPercent, int validatedTreeCount)
{
    ProgressPercent = progressPercent;
    ValidatedTreeCount = validatedTreeCount;
    if (progressPercent >= 100)
    {
        Status = ProjectStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }
}

// En el handler solo llamas al método:
project.UpdateProgress(request.ProgressPercent, request.ValidatedTreeCount);
await _context.SaveChangesAsync(ct);
```

### Regla 4 — IA de evidencias en background
```csharp
// En UploadEvidenceCommandHandler:
// 1. Subir archivo → obtener URL
// 2. Crear Evidence con AiValidationStatus = Pending
// 3. SaveChanges — el admin ya tiene respuesta
// 4. Disparar validación IA sin await (fuego y olvida)
_ = Task.Run(async () =>
{
    var result = await _aiService.ValidateEvidencePhotoAsync(evidence.FileUrl);
    evidence.SetAiResult(result.Status, result.EstimatedTreeCount, result.Notes);
    await _context.SaveChangesAsync();
}, CancellationToken.None);
```

## FluentValidation — reglas obligatorias por feature

### CreateQuoteCommandValidator
```csharp
RuleFor(x => x.TreeCount).GreaterThan(0)
    .WithMessage("El número de árboles debe ser mayor a cero.");
RuleFor(x => x.TerritoryId).NotEmpty()
    .WithMessage("El territorio es obligatorio.");
RuleFor(x => x.Sector).NotEmpty().MaximumLength(100)
    .WithMessage("El sector es obligatorio.");
```

### RegisterCompanyCommandValidator
```csharp
RuleFor(x => x.Email).NotEmpty().EmailAddress();
RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
RuleFor(x => x.NIT).NotEmpty().MaximumLength(20);
RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(200);
```

### LoginCommandValidator
```csharp
RuleFor(x => x.Email).NotEmpty().EmailAddress();
RuleFor(x => x.Password).NotEmpty();
```

## Excepciones — cuándo lanzar cada una

```csharp
// Entidad no encontrada → el Middleware la convierte en 404
throw new NotFoundException(nameof(Quote), request.QuoteId);

// Empresa intentando ver datos de otra → 403
throw new ForbiddenException("No tienes acceso a este proyecto.");

// Datos inválidos que FluentValidation no alcanzó → 400
throw new ValidationException("La cotización ya fue aprobada.");
```

## DTOs — cómo mapear a mano (sin AutoMapper)

```csharp
// En el QueryHandler, mapeo explícito:
return new QuoteDetailDto(
    Id:          quote.Id,
    Sector:      quote.Sector,
    TreeCount:   quote.TreeCount,
    Status:      quote.Status.ToString(),
    AiProposal:  quote.AiProposal,
    Territory:   quote.Territory.Name,
    RequestedAt: quote.RequestedAt
);

// Los DTOs son records:
public record QuoteDetailDto(
    Guid Id,
    string Sector,
    int TreeCount,
    string Status,
    string? AiProposal,
    string Territory,
    DateTime RequestedAt
);
```

## Advisor — cómo construir el prompt con datos reales

```csharp
// En AskAdvisorCommandHandler:
// PRIMERO busca los datos reales del proyecto
var project = await _context.Projects
    .Include(p => p.Quote)
        .ThenInclude(q => q.Territory)
    .Include(p => p.Company)
    .FirstOrDefaultAsync(p => p.CompanyId == request.CompanyId, ct)
    ?? throw new NotFoundException(nameof(Project), request.CompanyId);

// LUEGO pasa esos datos a la IA (nunca al revés)
var response = await _aiService.GetProjectAdvisorResponseAsync(
    companyName:     project.Company.Name,
    territory:       project.Quote.Territory.Name,
    treeCount:       project.Quote.TreeCount,
    progressPercent: project.ProgressPercent,
    userQuestion:    request.Question,
    ct:              ct
);
```

## DependencyInjection.cs de Application

```csharp
public static IServiceCollection AddApplication(
    this IServiceCollection services)
{
    services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

    services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

    services.AddTransient(typeof(IPipelineBehavior<,>), 
        typeof(ValidationBehavior<,>));
    services.AddTransient(typeof(IPipelineBehavior<,>), 
        typeof(LoggingBehavior<,>));

    return services;
}
```

## Checklist antes de hacer merge

- [ ] dotnet build sin errores
- [ ] Cada Command tiene su Handler (y Validator si tiene reglas)
- [ ] Cada Query tiene su Handler
- [ ] Ningún Handler inyecta clases concretas de Infrastructure
- [ ] Ningún endpoint retorna entidades de Domain (solo DTOs)
- [ ] La regla todo-o-nada de ApproveQuote usa un solo SaveChanges
- [ ] Cada query de empresa filtra por CompanyId
- [ ] La validación de IA de evidencia corre en background
