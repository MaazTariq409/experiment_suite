using Enterprise.Shared.Results;

namespace TP06B.Contracts;

public interface IProductCatalogImportService
{
    Task<OperationResult<ImportSummary>> ImportAsync(
        IReadOnlyList<ImportRow> rows,
        CancellationToken cancellationToken = default);
}
