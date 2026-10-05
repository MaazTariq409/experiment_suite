using Enterprise.Shared.Results;

namespace TP06A.Contracts;

public interface IEmployeeImportService
{
    Task<OperationResult<ImportSummary>> ImportAsync(
        IReadOnlyList<ImportRow> rows,
        CancellationToken cancellationToken = default);
}
