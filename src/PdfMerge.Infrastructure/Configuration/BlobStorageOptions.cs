using System.ComponentModel.DataAnnotations;

namespace PdfMerge.Infrastructure.Configuration;

/// <summary>
/// Mapeia a seção "BlobStorage" das App Settings / local.settings.json.
/// </summary>
public sealed class BlobStorageOptions
{
    public const string SectionName = "BlobStorage";

    [Required(ErrorMessage = "BlobStorage:ConnectionString é obrigatório.")]
    public string ConnectionString { get; set; } = string.Empty;

    [Required(ErrorMessage = "BlobStorage:InputContainerName é obrigatório.")]
    public string InputContainerName { get; set; } = string.Empty;
}
