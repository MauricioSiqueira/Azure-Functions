using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using PdfMerge.Application.Interfaces;
using PdfMerge.Infrastructure.Configuration;

namespace PdfMerge.Infrastructure.Storage;

public sealed class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;

    public BlobStorageService(IOptions<BlobStorageOptions> options)
    {
        var config = options.Value;
        var serviceClient = new BlobServiceClient(config.ConnectionString);
        _containerClient = serviceClient.GetBlobContainerClient(config.InputContainerName);
    }

    public async Task<bool> ExistsAsync(string blobName, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);
        var response = await blobClient.ExistsAsync(cancellationToken);
        return response.Value;
    }

    public async Task<Stream> DownloadAsync(string blobName, CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);

        var memoryStream = new MemoryStream();
        await blobClient.DownloadToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;
        return memoryStream;
        
    }

    public async Task<(Uri SasUri, DateTimeOffset ExpiresAt)> UploadAndGetSasUriAsync(
    string blobName,
    Stream content,
    CancellationToken cancellationToken = default)
    {
        await _containerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blobClient = _containerClient.GetBlobClient(blobName);

        content.Position = 0;
        await blobClient.UploadAsync(content, overwrite: true, cancellationToken);

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(20);

        if (!blobClient.CanGenerateSasUri)
        {
            throw new InvalidOperationException(
                "Não foi possível gerar SAS: o cliente de blob não tem credenciais de chave compartilhada. " +
                "Use uma connection string com AccountKey, ou implemente geração de User Delegation SAS.");
        }

        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerClient.Name,
            BlobName = blobName,
            Resource = "b", // "b" = blob específico
            ExpiresOn = expiresAt,
            ContentDisposition = $"attachment; filename=\"{blobName}\""
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        var sasUri = blobClient.GenerateSasUri(sasBuilder);

        return (sasUri, expiresAt);
    }
}
