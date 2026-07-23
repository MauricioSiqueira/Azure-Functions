using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using PdfMerge.Domain.Exceptions;

namespace PdfMerge.Functions.Middleware;

public sealed class ExceptionHandlingMiddleware : IFunctionsWorkerMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var httpContext = context.GetHttpContext();
            if (httpContext is null)
            {
                _logger.LogError(ex, "Erro não tratado em trigger não-HTTP.");
                throw;
            }

            var (statusCode, errorCode, message) = MapException(ex);
            _logger.LogError(ex, "Erro tratado na requisição HTTP: {ErrorCode}", errorCode);

            httpContext.Response.Clear();
            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json; charset=utf-8";

            var payload = JsonSerializer.Serialize(new
            {
                error = errorCode,
                message
            });

            await httpContext.Response.WriteAsync(payload);
        }
    }

    private static (int StatusCode, string ErrorCode, string Message) MapException(Exception ex) => ex switch
    {
        InvalidMergeRequestException => ((int)HttpStatusCode.BadRequest, "invalid_request", ex.Message),
        BlobNotFoundException => ((int)HttpStatusCode.NotFound, "file_not_found", ex.Message),
        InvalidPdfException => ((int)HttpStatusCode.UnprocessableEntity, "invalid_pdf", ex.Message),
        _ => ((int)HttpStatusCode.InternalServerError, "internal_error",
              "Ocorreu um erro inesperado ao processar a requisição.")
    };
}
