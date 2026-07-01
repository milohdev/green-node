using Amazon.S3;
using Amazon.SimpleEmail;
using GreenNode.Application.Common.Interfaces;
using GreenNode.Infrastructure.Persistence;
using GreenNode.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GreenNode.Infrastructure;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // QuestPDF community license
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        // EF Core + Npgsql
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IApplicationDbContext>(p =>
            p.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<DatabaseSeeder>();

        // AWS SDK
        var awsOptions = configuration.GetAWSOptions();
        awsOptions.Credentials = new Amazon.Runtime.BasicAWSCredentials(
            configuration["AWS:AccessKey"],
            configuration["AWS:SecretKey"]
        );
        services.AddDefaultAWSOptions(awsOptions);
        services.AddAWSService<IAmazonS3>();
        services.AddAWSService<IAmazonSimpleEmailService>();

        // HttpClient para AiService
        services.AddHttpClient<AiService>();

        // Servicios
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IClaudeAiService, AiService>();
        services.AddScoped<IStorageService, StorageService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICertificateService, CertificateService>();

        return services;
    }
}
