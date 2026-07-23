namespace PdfMerge.Domain.Exceptions;

/// <summary>
/// Lançada quando a lista de hashes/nomes de arquivo informada é inválida
/// (vazia, nula, duplicada além do permitido, excede o limite máximo, etc).
/// </summary>
public sealed class InvalidMergeRequestException : Exception
{
    public InvalidMergeRequestException(string message) : base(message)
    {
    }
}
