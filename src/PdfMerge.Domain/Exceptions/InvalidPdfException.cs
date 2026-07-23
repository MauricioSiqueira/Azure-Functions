namespace PdfMerge.Domain.Exceptions;

/// <summary>
/// Lançada quando um arquivo baixado do blob não é um PDF válido/legível.
/// </summary>
public sealed class InvalidPdfException : Exception
{
    public string BlobName { get; }

    public InvalidPdfException(string blobName, Exception innerException)
        : base($"Arquivo '{blobName}' não pôde ser lido como PDF válido.", innerException)
    {
        BlobName = blobName;
    }
}
