namespace PdfMerge.Application.Interfaces;

public interface IBlobStorageService
{
    Task<bool> ExistsAsync(string blobName, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string blobName, CancellationToken cancellationToken = default);
    Task<(Uri SasUri, DateTimeOffset ExpiresAt)> UploadAndGetSasUriAsync(
        string blobName,
        Stream content,
        CancellationToken cancellationToken = default);
}
