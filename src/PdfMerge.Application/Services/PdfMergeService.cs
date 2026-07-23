using Microsoft.Extensions.Logging;
using PdfMerge.Application.DTOs;
using PdfMerge.Application.Interfaces;
using PdfMerge.Application.Validators;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Parsing;

namespace PdfMerge.Application.Services;

public sealed class PdfMergeService(IBlobStorageService blobStorageService, ILogger<PdfMergeService> logger) : IPdfMergeService
{
    public async Task<MergePdfsResult> MergeAsync(MergePdfsRequest request, CancellationToken cancellationToken = default)
    {
        MergePdfsRequestValidator.Validate(request);

        using var outputDocument = new PdfDocument();
        var downloadedStreams = new List<Stream>();

        try
        {
            foreach (var fileHash in request.FileHashes)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var exists = await blobStorageService.ExistsAsync(fileHash, cancellationToken);
                if(!exists)
                {
                    logger.LogError($"file {fileHash} don't exist in azure blob!");
                    continue;
                }
                    
                var blobStream = await blobStorageService.DownloadAsync(fileHash, cancellationToken);
                logger.LogInformation($"Download file : {fileHash}");

                downloadedStreams.Add(blobStream);
                logger.LogDebug($"Add {fileHash} to the stream list");
            }
            
            logger.LogInformation("Convert a list of streams into a list of PdfDocumentBase");
            var loadedDocuments = downloadedStreams.Select(stream => new PdfLoadedDocument(stream)).Cast<PdfDocumentBase>().ToArray();
            
            logger.LogInformation("Merge list of Pdf");
            PdfDocumentBase.Merge(outputDocument, loadedDocuments);

            using var resultStream = new MemoryStream();
            outputDocument.Save(resultStream);
            resultStream.Position = 0;

            var fileName = string.IsNullOrWhiteSpace(request.OutputFileName)
                ? $"merged-{DateTime.UtcNow:yyyyMMddHHmmss}.pdf"
                : EnsurePdfExtension(request.OutputFileName!);

            var blobName = $"{fileName}";

            logger.LogDebug("");
            var (sasUri, expiresAt) = await blobStorageService.UploadAndGetSasUriAsync(
            blobName, resultStream, cancellationToken);    
            
            return new MergePdfsResult
            {
                Uri = sasUri,
                Nome = fileName,
            };
        }
        finally
        {
            foreach (var stream in downloadedStreams)
            {
                await stream.DisposeAsync();
            }
        }
    }

    private static string EnsurePdfExtension(string fileName)
    {
        return fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
            ? fileName
            : $"{fileName}.pdf";
    }
}
