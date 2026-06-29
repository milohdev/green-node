namespace GreenNode.Application.Common.Interfaces;

public interface IStorageService
{
    Task<string> UploadEvidenceAsync(
        Stream fileStream, string fileName, string contentType,
        CancellationToken ct = default);

    Task<string> UploadCertificateAsync(
        Stream pdfStream, string fileName,
        CancellationToken ct = default);

    Task<string> GeneratePresignedUrlAsync(
        string key, string bucketType,
        CancellationToken ct = default);

    Task DeleteFileAsync(
        string key, string bucketType,
        CancellationToken ct = default);
}
