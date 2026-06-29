namespace GreenNode.Application.Common.Interfaces;

public interface IStorageService
{
    Task<string> UploadEvidenceAsync(Stream fileStream, string fileName, string contentType);
    Task<string> UploadCertificateAsync(Stream pdfStream, string fileName);
    Task<string> GeneratePresignedUrlAsync(string key, string bucketType);
    Task DeleteFileAsync(string key, string bucketType);
}
