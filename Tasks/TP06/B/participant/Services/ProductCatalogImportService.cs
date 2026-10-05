using Enterprise.Shared.Results;
using TP06B.Contracts;

namespace TP06B.Participant.Services;

/// <summary>PARTICIPANT IMPLEMENTATION AREA.</summary>
public sealed class ProductCatalogImportService : IProductCatalogImportService
{
    public Task<OperationResult<ImportSummary>> ImportAsync(
        IReadOnlyList<ImportRow> rows,
        CancellationToken cancellationToken = default)
    {
        // TODO: parse Key,Name,Value CSV; continue on row errors; enforce unique keys (case-insensitive)
        throw new NotImplementedException("Implement file import validation for TP06-B.");
    }
}
