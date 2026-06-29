using Amazon.S3;
using Amazon.S3.Model;
using GreenNode.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GreenNode.Infrastructure.Services;

public class StorageService(IAmazonS3 amazonS3, IConfiguration configuration, ILogger<StorageService> logger)
    : IStorageService
{
    private string EvidencesBucket => configuration["AWS:EvidencesBucket"]
        ?? throw new InvalidOperationException("AWS:EvidencesBucket no configurado.");

    private string CertificatesBucket => configuration["AWS:CertificatesBucket"]
        ?? throw new InvalidOperationException("AWS:CertificatesBucket no configurado.");

    public async Task<string> UploadEvidenceAsync(Stream fileStream, string fileName, string contentType)
    {
        try
        {
            var key = $"evidencias/{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}/{fileName}";
            var request = new PutObjectRequest
            {
                BucketName = EvidencesBucket,
                Key = key,
                InputStream = fileStream,
                ContentType = contentType,
                CannedACL = S3CannedACL.NoACL
            };
            await amazonS3.PutObjectAsync(request);
            return key;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error subiendo evidencia a S3: {FileName}", fileName);
            throw;
        }
    }

    public async Task<string> UploadCertificateAsync(Stream pdfStream, string fileName)
    {
        try
        {
            var key = $"certificados/{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid()}/{fileName}";
            var request = new PutObjectRequest
            {
                BucketName = CertificatesBucket,
                Key = key,
                InputStream = pdfStream,
                ContentType = "application/pdf",
                CannedACL = S3CannedACL.NoACL
            };
            await amazonS3.PutObjectAsync(request);
            return key;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error subiendo certificado a S3: {FileName}", fileName);
            throw;
        }
    }

    public async Task<string> GeneratePresignedUrlAsync(string key, string bucketType)
    {
        var bucket = bucketType == "certificados" ? CertificatesBucket : EvidencesBucket;
        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucket,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.AddSeconds(3600)
        };
        return await Task.FromResult(amazonS3.GetPreSignedURL(request));
    }

    public async Task DeleteFileAsync(string key, string bucketType)
    {
        try
        {
            var bucket = bucketType == "certificados" ? CertificatesBucket : EvidencesBucket;
            var request = new DeleteObjectRequest
            {
                BucketName = bucket,
                Key = key
            };
            await amazonS3.DeleteObjectAsync(request);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error eliminando archivo de S3 (ignorado): {Key}", key);
        }
    }
}
