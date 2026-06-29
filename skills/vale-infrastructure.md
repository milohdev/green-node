# Skill — Vale · Infrastructure

## Tu rol en GreenNode

Eres la sala de máquinas. Todo lo que el sistema necesita del mundo 
exterior pasa por ti: base de datos, IA, almacenamiento en la nube, 
emails y generación de PDFs. Cami te dice qué necesita a través de 
interfaces. Tú las implementas sin que Application sepa los detalles.

## Lo que tocas — y solo tú tocas

- src/GreenNode.Infrastructure/ completo
- docker-compose.yml y docker-compose.override.yml
- Migraciones de EF Core
- DatabaseSeeder.cs

## Lo que NUNCA tocas

- src/GreenNode.Application/ — esas interfaces las define Cami
- src/GreenNode.Domain/ — esas entidades las acordaron los 3
- src/GreenNode.WebAPI/ — eso es de Juan

## Reglas de EF Core en este proyecto

### Configuraciones Fluent API
Cada entidad tiene su propio archivo en Persistence/Configurations/.
NUNCA uses DataAnnotations ([Required], [MaxLength]) en las entidades.
Todo va en Fluent API dentro de IEntityTypeConfiguration<T>.

Plantilla obligatoria:
```csharp
public class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.ToTable("Quotes");
        builder.HasKey(q => q.Id);
        // propiedades, índices, relaciones aquí
    }
}
```

ApplicationDbContext usa ApplyConfigurationsFromAssembly() — no registres 
configuraciones manualmente, EF las descubre solo.

### Enums → string en la DB
Todos los enums se guardan como string, nunca como int:
```csharp
builder.Property(q => q.Status).HasConversion<string>();
```

### Relaciones y delete behavior
- Cascade: solo Evidence → Project (si borro el proyecto, borro sus evidencias)
- Restrict en todo lo demás (nunca borrar en cascada Companies, Quotes, Territories)

### Constraints en DB
TreeCount > 0 va como check constraint en QuoteConfiguration:
```csharp
builder.ToTable(t => t.HasCheckConstraint("CK_Quotes_TreeCount", "\"TreeCount\" > 0"));
```

### Índices únicos obligatorios
- Users: Email único
- Companies: NIT único, UserId único (1:1 con User)
- Projects: QuoteId único (una cotización → un proyecto)
- Certificates: ProjectId único, VerificationCode único

### Precisión decimal
- ProgressPercent: HasPrecision(5, 2)
- EstimatedCO2Tons: HasPrecision(10, 4)

## Cómo crear y aplicar migraciones

```bash
# SIEMPRE desde la raíz del repo
dotnet ef migrations add NombreMigracion \
  --project src/GreenNode.Infrastructure \
  --startup-project src/GreenNode.WebAPI \
  --output-dir Persistence/Migrations

dotnet ef database update \
  --project src/GreenNode.Infrastructure \
  --startup-project src/GreenNode.WebAPI
```

Nombre de migración en PascalCase que describa el cambio:
InitialCreate, AddAiProposalToQuote, AddVerificationCodeIndex

## Cómo implementar los servicios externos

### Plantilla base para cualquier servicio
```csharp
public class AiService : IAiService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AiService> _logger;

    public AiService(IConfiguration configuration, ILogger<AiService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    // implementación
}
```

### AiService — los 3 métodos que debes implementar
1. GenerateQuoteProposalAsync(sector, treeCount, territory)
   → llama a la IA con un prompt sobre el sector y territorio
   → devuelve string con el marco lógico de la propuesta

2. ValidateEvidencePhotoAsync(fileUrl)
   → envía la URL de la foto a la IA con visión
   → devuelve AiEvidenceResult(Status, EstimatedTreeCount, Notes)
   → Status puede ser Validated o Rejected

3. GetProjectAdvisorResponseAsync(companyName, territory, treeCount, progressPercent, userQuestion)
   → construye un prompt con los datos REALES del proyecto
   → devuelve string con la respuesta del asesor
   → NUNCA responder sin los datos del proyecto (evita respuestas genéricas)

### StorageService
- Subida directa a la nube (S3 o equivalente)
- Devuelve la URL pública del archivo subido
- NUNCA guarda el archivo en el servidor local

### EmailService
3 métodos: SendQuoteReceivedAsync, SendQuoteApprovedAsync, SendQuoteRejectedAsync
Usa el email de Company.ContactEmail como destino.

### CertificateService
- Genera PDF con: nombre empresa, territorio, cantidad árboles, 
  CO₂ estimado, fecha de emisión, código de verificación
- Devuelve Stream del PDF
- El Handler de GenerateCertificate guarda el Stream en Storage 
  y registra la URL en Certificate.FileUrl

## DatabaseSeeder — orden de ejecución

El seeder siempre verifica antes de insertar (si ya existe, no hace nada):

1. Territories (5 territorios colombianos)
2. Admin User (admin@greennode.com)
3. Datos de demo:
   - Company "EcoFuturo S.A.S." con su User
   - Quote aprobada con AiProposal llena
   - Project activo (ProgressPercent: 40, ValidatedTreeCount: 200)
   - Evidence validada por IA
   (El certificado se genera en el día 5 desde el endpoint, no en el seeder)

## DependencyInjection.cs de Infrastructure

Registra todo en un método de extensión:
```csharp
public static IServiceCollection AddInfrastructure(
    this IServiceCollection services, IConfiguration configuration)
{
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
            b => b.MigrationsAssembly("GreenNode.Infrastructure")));

    services.AddScoped<IApplicationDbContext>(p =>
        p.GetRequiredService<ApplicationDbContext>());

    services.AddScoped<IAiService, AiService>();
    services.AddScoped<IStorageService, StorageService>();
    services.AddScoped<IEmailService, EmailService>();
    services.AddScoped<ICertificateService, CertificateService>();

    return services;
}
```

## Checklist antes de hacer merge

- [ ] dotnet build sin errores
- [ ] docker-compose up -d levanta sin errores  
- [ ] dotnet ef database update aplica sin errores
- [ ] Las 7 tablas existen en PostgreSQL
- [ ] El seeder sembró territorios y admin
- [ ] Los servicios externos tienen su implementación (aunque sea stub)
- [ ] No hay strings de conexión reales en el código (solo en .env)

---

## AWS — Configuración y uso

### SDK de AWS en DependencyInjection.cs de Infrastructure

```csharp
// Agrega esto en AddInfrastructure():
services.AddDefaultAWSOptions(configuration.GetAWSOptions());
services.AddAWSService<IAmazonS3>();
services.AddAWSService<IAmazonSimpleEmailService>();
```

Las credenciales (AWS_ACCESS_KEY_ID, AWS_SECRET_ACCESS_KEY) 
se leen automáticamente desde variables de entorno o desde 
el perfil de AWS configurado en la máquina.
NUNCA las hardcodees en el código.

### StorageService — dos buckets según tipo de archivo

```csharp
public class StorageService : IStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly IConfiguration _config;

    // Para evidencias (fotos):
    private string BucketEvidencias => 
        _config["AWS:S3:BucketEvidencias"]!;

    // Para certificados (PDFs):
    private string BucketCertificados => 
        _config["AWS:S3:BucketCertificados"]!;

    // Sube una evidencia fotográfica
    public async Task<string> UploadEvidenceAsync(
        Stream stream, string fileName, string contentType,
        CancellationToken ct = default)
    {
        // Sube a greennode-evidencias
        // Devuelve la URL del objeto en S3
    }

    // Sube un certificado PDF
    public async Task<string> UploadCertificateAsync(
        Stream stream, string fileName,
        CancellationToken ct = default)
    {
        // Sube a greennode-certificados
        // Devuelve la URL del objeto en S3
    }
}
```

NOTA: Actualiza también IStorageService.cs en Application 
para tener los dos métodos separados:
- UploadEvidenceAsync(stream, fileName, contentType)
- UploadCertificateAsync(stream, fileName)

### EmailService — usa SES

```csharp
public class EmailService : IEmailService
{
    private readonly IAmazonSimpleEmailService _ses;
    private readonly string _fromEmail;

    // El remitente siempre es:
    // greennode.notificaciones@gmail.com
    // (debe estar verificado en SES)
}
```

### Región AWS
Todos los servicios usan: us-east-2
