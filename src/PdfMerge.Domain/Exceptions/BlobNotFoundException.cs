namespace PdfMerge.Domain.Exceptions;

/// <summary>
/// Lançada quando um dos hashes/nomes de arquivo informados não é encontrado no Blob Storage.
/// </summary>
public sealed class BlobNotFoundException : Exception
{
    public string BlobName { get; }

    public BlobNotFoundException(string blobName)
        : base($"Arquivo '{blobName}' não foi encontrado no container de origem.")
    {
        BlobName = blobName;
    }
}
