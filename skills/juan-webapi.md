# Skill — Juan · WebAPI

## Tu rol en GreenNode

Eres la puerta de entrada. Todo lo que el mundo exterior toca pasa 
por ti. Recibes la petición HTTP, validas que quien llama tiene 
permiso, se la pasas a MediatR, y devuelves la respuesta. 
Nada más. La lógica de negocio no es tu problema.

## Lo que tocas — y solo tú tocas

- src/GreenNode.WebAPI/ completo
- Controllers/, Hubs/, Middlewares/
- Program.cs, appsettings.json
- Configuración JWT, SignalR, OpenAPI/Scalar
- CORS, rate limiting, variables de entorno en producción

## Lo que NUNCA tocas

- src/GreenNode.Application/ — eso es de Cami
- src/GreenNode.Infrastructure/ — eso es de Vale
- src/GreenNode.Domain/ — cambios solo con aprobación del equipo

## Controllers — la plantilla obligatoria

Un controller solo hace tres cosas: extraer datos del request, 
llamar a MediatR, devolver el resultado. Nada más.

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // JWT obligatorio por defecto en todos
public class QuotesController : ControllerBase
{
    private readonly ISender _sender;

    public QuotesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [Authorize(Roles = "Company")]
    public async Task<IActionResult> Create(
        CreateQuoteDto dto, CancellationToken ct)
    {
        // Extrae CompanyId del JWT — nunca del body ni de la URL
        var companyId = Guid.Parse(User.FindFirstValue("companyId")!);

        var result = await _sender.Send(
            new CreateQuoteCommand(companyId, dto.TerritoryId, 
                                   dto.Sector, dto.TreeCount), ct);

        return Ok(new { data = result, message = "Cotización creada." });
    }
}
```

### Reglas de los Controllers

- Inyecta ISender (de MediatR), no IMediator
- NUNCA inyectes DbContext, servicios de Infrastructure ni entidades de Domain
- El CompanyId SIEMPRE viene del JWT: User.FindFirstValue("companyId")
- NUNCA confíes en un Id de empresa que venga en el body o la URL
- Siempre devuelve: { data: ..., message: ... }
- Los errores los maneja el Middleware, tú no hagas try/catch en controllers

## JWT — configuración y claims

### Configuración en Program.cs
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["JwtSettings:Secret"]!))
        };
    });
```

### Claims obligatorios en el token
El LoginCommandHandler (Cami) genera el token. 
Tú defines qué claims necesitas — Cami los incluye:

```csharp
// Claims que DEBES pedir a Cami que incluya en el JWT:
new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
new Claim(ClaimTypes.Email, user.Email),
new Claim(ClaimTypes.Role, user.Role.ToString()),
new Claim("companyId", company?.Id.ToString() ?? "") // vacío si es Admin
```

### Cómo leer claims en el Controller
```csharp
var userId    = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
var companyId = Guid.Parse(User.FindFirstValue("companyId")!);
var role      = User.FindFirstValue(ClaimTypes.Role);
var isAdmin   = User.IsInRole("Admin");
```

### Endpoints públicos (sin [Authorize])
Solo estos dos no necesitan JWT:
- POST /api/auth/register
- POST /api/auth/login

Todos los demás llevan [Authorize] mínimo.

## Roles — quién puede qué

```csharp
// Solo admin
[Authorize(Roles = "Admin")]

// Solo empresa
[Authorize(Roles = "Company")]

// Ambos (admin ve todo, empresa ve solo lo suyo)
[Authorize] // el filtro por empresa lo hace el Handler de Cami
```

## Formato de respuesta — siempre consistente

```csharp
// Éxito
return Ok(new { data = result, message = "Operación exitosa." });
return CreatedAtAction(nameof(GetById), new { id = result.Id }, 
    new { data = result, message = "Recurso creado." });

// Sin contenido
return NoContent();

// Los errores NO van aquí — van en el Middleware
```

## ExceptionHandlingMiddleware — cómo implementarlo

```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next, 
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = 404;
            await context.Response.WriteAsJsonAsync(new 
            { 
                message = ex.Message, 
                errors = Array.Empty<string>() 
            });
        }
        catch (ForbiddenException ex)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new 
            { 
                message = ex.Message,
                errors = Array.Empty<string>()
            });
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new 
            { 
                message = "Datos inválidos.",
                errors = ex.Errors
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no controlado.");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new 
            { 
                message = "Ocurrió un error interno. Intenta de nuevo.",
                errors = Array.Empty<string>()
            });
        }
    }
}
```

## SignalR — DashboardHub

```csharp
[Authorize(Roles = "Admin")]
public class DashboardHub : Hub
{
    // Los admins se conectan aquí para recibir actualizaciones en tiempo real
    // Cuando cambia el estado de una cotización o proyecto,
    // el Handler correspondiente (Cami) notifica a través de este Hub

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
        await base.OnConnectedAsync();
    }
}
```

### Cómo notificar desde un Handler
Cami inyecta IHubContext en sus Handlers para notificar:
```csharp
// Esto lo hace Cami en ApproveQuoteCommandHandler
await _hubContext.Clients.Group("admins")
    .SendAsync("QuoteApproved", new { quoteId, projectId }, ct);
```

Los eventos que disparan notificación SignalR:
- QuoteApproved → cuando se aprueba una cotización
- QuoteRejected → cuando se rechaza
- EvidenceValidated → cuando la IA termina de validar una foto
- ProjectCompleted → cuando el progreso llega a 100%

## OpenAPI + Scalar en Program.cs

```csharp
// Registro
builder.Services.AddOpenApi();

// Mapeo (solo en Development)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();            // JSON en /openapi/v1.json
    app.MapScalarApiReference(); // UI  en /scalar/v1
}
```

NO uses Swashbuckle. NO uses app.UseSwagger(). 
En .NET 10 OpenAPI es nativo.

## Program.cs — orden correcto de middleware

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Servicios
builder.Services.AddApplication();      // extensión de Cami
builder.Services.AddInfrastructure(     // extensión de Vale
    builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddAuthentication(...).AddJwtBearer(...);
builder.Services.AddAuthorization();
builder.Services.AddSignalR();

var app = builder.Build();

// 2. Seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();
    await DatabaseSeeder.SeedAsync(db);
}

// 3. Pipeline — el orden importa
app.UseMiddleware<ExceptionHandlingMiddleware>(); // primero
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseHttpsRedirection();
app.UseAuthentication(); // antes de Authorization
app.UseAuthorization();
app.MapControllers();
app.MapHub<DashboardHub>("/hubs/dashboard");

app.Run();
```

## Endpoints — tabla completa de la API

| Método | Ruta | Rol | Handler |
|--------|------|-----|---------|
| POST | /api/auth/register | Público | RegisterCompanyCommand |
| POST | /api/auth/login | Público | LoginCommand |
| GET | /api/quotes | Admin | GetQuotesQuery |
| POST | /api/quotes | Company | CreateQuoteCommand |
| GET | /api/quotes/{id} | Admin | GetQuoteByIdQuery |
| POST | /api/quotes/{id}/approve | Admin | ApproveQuoteCommand |
| POST | /api/quotes/{id}/reject | Admin | RejectQuoteCommand |
| GET | /api/projects | Admin+Company | GetProjectsQuery |
| GET | /api/projects/{id} | Admin+Company | GetProjectByIdQuery |
| PUT | /api/projects/{id}/progress | Admin | UpdateProgressCommand |
| POST | /api/evidences | Admin | UploadEvidenceCommand |
| GET | /api/evidences/{projectId} | Admin+Company | GetEvidencesByProjectQuery |
| GET | /api/companies | Admin | GetCompaniesQuery |
| GET | /api/companies/{id} | Admin | GetCompanyByIdQuery |
| PUT | /api/companies/{id} | Admin | UpdateCompanyCommand |
| GET | /api/certificates/{projectId} | Admin+Company | GetCertificateByProjectQuery |
| POST | /api/certificates/{projectId} | Admin | GenerateCertificateCommand |
| GET | /api/dashboard/summary | Admin | GetDashboardSummaryQuery |
| POST | /api/advisor | Company | AskAdvisorCommand |

## Límite de tamaño para subida de archivos

```csharp
// En Program.cs
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10 MB
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
});
```

## Checklist antes de hacer merge

- [ ] dotnet build sin errores
- [ ] /scalar/v1 abre sin errores en Development
- [ ] Todos los endpoints protegidos retornan 401 sin token
- [ ] Admin no puede hacer peticiones de Company y viceversa
- [ ] El CompanyId nunca se toma del body ni de la URL
- [ ] ExceptionHandlingMiddleware registrado como primero en el pipeline
- [ ] SignalR Hub accesible en /hubs/dashboard
- [ ] No hay lógica de negocio en ningún Controller
- [ ] appsettings.Development.json tiene JwtSettings completo
- [ ] El .env real no está commiteado
