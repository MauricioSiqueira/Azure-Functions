using PdfMerge.Application.DTOs;
using PdfMerge.Domain.Exceptions;

namespace PdfMerge.Application.Validators;

public static class MergePdfsRequestValidator
{
    public static void Validate(MergePdfsRequest? request)
    {
        if (request is null)
            throw new InvalidMergeRequestException("O corpo da requisição não pôde ser lido ou está vazio.");

        if (request.FileHashes is null || request.FileHashes.Count == 0)
            throw new InvalidMergeRequestException("A lista 'fileHashes' é obrigatória e não pode estar vazia.");

        if (request.FileHashes.Any(string.IsNullOrWhiteSpace))
            throw new InvalidMergeRequestException("A lista 'fileHashes' contém itens vazios ou nulos.");
    }
}
