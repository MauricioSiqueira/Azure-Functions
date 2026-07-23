using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using PdfMerge.Application.DTOs;
using PdfMerge.Application.Interfaces;
using PdfMerge.Domain.Exceptions;

namespace PdfMerge.Functions.Http;

public sealed class MergePdfsFunction(IPdfMergeService pdfMergeService, ILogger<MergePdfsFunction> logger)
{
    [Function("MergePdfs")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "merge")] HttpRequest req)
    {
        logger.LogInformation("Convert a Json into a MergePdfsRequest object!");
        var request = await req.ReadFromJsonAsync<MergePdfsRequest>();

        try
        {
            logger.LogInformation("Call the merge function!");
            var result = await pdfMergeService.MergeAsync(request!);
            return new OkObjectResult(result);
        }
        catch
        {
            throw new InvalidMergeRequestException("Error in merge PDFs!");
        }
    }
}
