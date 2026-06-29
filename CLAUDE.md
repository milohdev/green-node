# GreenNode — Guía para Claude Code

## Qué es este proyecto

API REST en .NET 10 con Clean Architecture para gestión de siembra 
de árboles. Las empresas solicitan cotizaciones, un admin las aprueba, 
se crean proyectos, se suben evidencias fotográficas que valida una IA, 
y al completarse se genera un certificado PDF.

## Stack

- .NET 10 / C# 13
- PostgreSQL 16 + EF Core 10 (Npgsql)
- MediatR 12 (CQRS)
- FluentValidation 11
- JWT Bearer 10
- SignalR (tiempo real)
- OpenAPI nativo .NET 10 + Scalar (NO Swashbuckle)
- Docker solo para la DB
- BCrypt.Net-Next para hashing de passwords

## Arquitectura — 4 capas, dependencias en una sola dirección

Domain ← Application ← Infrastructure
                     ← WebAPI

- Domain: no depende de nadie. Entidades, enums, reglas de negocio puras.
- Application: depende solo de Domain. CQRS, interfaces, validaciones, DTOs.
- Infrastructure: implementa las interfaces de Application. DB, IA, Storage, Email, PDF.
- WebAPI: orquesta todo. Controllers, JWT, SignalR, Middlewares, OpenAPI.

## Regla de oro de dependencias

Si estás en Domain y estás a punto de hacer using de algo que no sea 
el propio Domain → PARA. Algo está mal.

Si estás en Application y estás a punto de hacer using de Infrastructure 
o WebAPI → PARA. Application solo conoce Domain y sus propias interfaces.

## Carriles del equipo — quién toca qué

| Archivo/Carpeta | Dueño | Los otros NO tocan |
|---|---|---|
| src/GreenNode.Domain/ | Los 3 juntos | Cambios solo con aprobación del equipo |
| src/GreenNode.Application/ | Cami | Juan y Vale no modifican |
| src/GreenNode.Infrastructure/ | Vale | Juan y Cami no modifican |
| src/GreenNode.WebAPI/ | Juan | Cami y Vale no modifican |

## CQRS — estructura obligatoria en Application

Cada feature vive en src/GreenNode.Application/Features/{Feature}/

Commands → mutan estado (crear, aprobar, rechazar, subir, actualizar)
Queries  → solo leen (listar, obtener por id, dashboard, resumen)

Cada Command o Query tiene exactamente 3 archivos:
- {Nombre}Command.cs o {Nombre}Query.cs      → el request (record con IRequest<T>)
- {Nombre}CommandHandler.cs o QueryHandler.cs → implementa IRequestHandler<T>
- {Nombre}CommandValidator.cs (solo Commands) → FluentValidation, si tiene reglas

Nunca pongas lógica de negocio en un Handler de Query.
Nunca hagas una consulta a la DB desde un CommandHandler sin pasar por IApplicationDbContext.

## Entidades del Domain — reglas de escritura

- Heredan de BaseEntity (Id: Guid, CreatedAt: DateTime)
- Propiedades con private set
- Constructor público con parámetros requeridos
- Constructor protegido sin parámetros para EF Core: protected Entity() { }
- Colecciones inicializadas con []: public ICollection<Evidence> Evidences { get; private set; } = [];
- Métodos de dominio para mutar estado: Approve(), Reject(), SetAiProposal(), UpdateProgress()
- NUNCA setters públicos en entidades

## Flujo principal — prioridad absoluta

cotización → proyecto → evidencia

Si algo se atrasa, este flujo va primero. Las features de IA, dashboard 
y certificado son importantes pero secundarias al flujo principal.

## Reglas de negocio críticas — nunca romper estas

1. Al aprobar una cotización: Quote.Status = Approved + Project creado 
   en la MISMA transacción. Si falla cualquiera de los dos, no pasa nada. 
   Nunca dejar Quote aprobada sin Project, ni Project sin Quote aprobada.

2. Una empresa NUNCA puede ver proyectos, cotizaciones ni evidencias 
   de otra empresa. Cada query filtra por CompanyId del usuario autenticado.
   NUNCA confiar en un Id que llega en la URL sin validar que pertenece 
   al usuario autenticado.

3. Si ProgressPercent llega a 100 → Project.Status = Completed automáticamente.
   Esto pasa en el domain method UpdateProgress(), no en el Handler.

4. TreeCount siempre > 0. Validado en FluentValidation Y en constraint de DB.

5. La validación de IA de evidencias corre en background (fuego y olvida).
   El admin no espera. Evidence.AiValidationStatus empieza en Pending 
   y se actualiza cuando la IA termina.

## Seguridad

- JWT obligatorio en todos los endpoints excepto /auth/register y /auth/login
- Roles: Admin puede todo. Company solo ve sus propios datos.
- El CompanyId del usuario autenticado viene del JWT, nunca de la URL.
- Middleware de excepciones convierte: 
    NotFoundException     → 404
    ForbiddenException    → 403  
    ValidationException   → 400
    Cualquier otra        → 500 con mensaje genérico (nunca stack trace)

## Cómo correr el proyecto

```bash
# Levantar la DB
docker-compose up -d

# Correr la API (desde la raíz)
dotnet run --project src/GreenNode.WebAPI

# UI de documentación
http://localhost:5000/scalar/v1

# Compilar todo
dotnet build GreenNode.slnx
```

## Migraciones EF Core

```bash
# Crear migración
dotnet ef migrations add NombreMigracion \
  --project src/GreenNode.Infrastructure \
  --startup-project src/GreenNode.WebAPI \
  --output-dir Persistence/Migrations

# Aplicar
dotnet ef database update \
  --project src/GreenNode.Infrastructure \
  --startup-project src/GreenNode.WebAPI
```

## Qué NO hacer — lista roja

- NO usar Swashbuckle. La doc va con Microsoft.AspNetCore.OpenApi + Scalar.
- NO poner lógica de negocio en Controllers. Los controllers solo llaman a MediatR.
- NO inyectar DbContext directamente en Application. Solo a través de IApplicationDbContext.
- NO retornar entidades de Domain desde los endpoints. Siempre DTOs.
- NO hacer AutoMapper. Los DTOs se mapean a mano en los QueryHandlers.
- NO usar var para tipos no obvios. Preferir tipos explícitos.
- NO dejar Console.WriteLine en código que va a producción. Usar ILogger.
- NO commitear el archivo .env con valores reales. Solo .env.example.
- NO agregar paquetes NuGet sin avisar al equipo en el chat.

## Convenciones de nombres

- Entidades:    PascalCase singular (Quote, Project, Evidence)
- Interfaces:   I + PascalCase (IApplicationDbContext, IAiService)
- Commands:     Verbo + Sustantivo + Command (ApproveQuoteCommand)
- Queries:      Get + Sustantivo + Query (GetQuoteByIdQuery)
- Handlers:     mismo nombre del Command/Query + Handler
- Validators:   mismo nombre del Command + Validator
- DTOs:         Sustantivo + Dto (QuoteDto, ProjectDetailDto)
- Controllers:  Sustantivo plural + Controller (QuotesController)
- Endpoints:    kebab-case en minúsculas (/api/quotes, /api/projects)

## Formato de respuesta de la API

Todos los endpoints devuelven:
```json
{
  "data": { },
  "message": "string"
}
```

Los errores devuelven:
```json
{
  "message": "string",
  "errors": [ ]
}
```

## IA — los 3 usos que tiene en el sistema

1. GenerateQuoteProposal → recibe sector, treeCount, territory → devuelve texto (marco lógico)
2. ValidateEvidencePhoto → recibe fileUrl de la foto → devuelve status + estimado de árboles + notas
3. GetProjectAdvisorResponse → recibe datos reales del proyecto + pregunta del usuario → devuelve respuesta contextualizada

Implementados en Infrastructure/Services/AiService.cs.
Las interfaces viven en Application/Common/Interfaces/IAiService.cs.
Application NUNCA sabe qué proveedor de IA usa. Solo conoce la interfaz.

## Features por día — para saber qué va primero

Día 1: Auth (register + login) + estructura DB
Día 2: Quotes completo (crear, listar, aprobar, rechazar) + IA propuesta + emails
Día 3: Projects + Evidences + validación IA en background + seguridad por empresa
Día 4: Dashboard + SignalR + Certificates PDF + despliegue
Día 5: Pruebas, seed de demo, pulir mensajes de error
