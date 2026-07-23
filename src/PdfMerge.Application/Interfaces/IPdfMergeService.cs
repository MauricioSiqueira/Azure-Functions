using PdfMerge.Application.DTOs;

namespace PdfMerge.Application.Interfaces;

public interface IPdfMergeService
{
    Task<MergePdfsResult> MergeAsync(MergePdfsRequest request, CancellationToken cancellationToken = default);
}
