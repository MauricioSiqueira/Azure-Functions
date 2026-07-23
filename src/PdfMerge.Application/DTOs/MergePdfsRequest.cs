using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PdfMerge.Application.DTOs;

public sealed class MergePdfsRequest
{
    [JsonPropertyName("fileHashes")]
    [Required]
    public List<string> FileHashes { get; set; } = [];

    [JsonPropertyName("outputFileName")]
    [Required]
    public string? OutputFileName { get; set; }
}
