using Enterprise.Shared.Results;

namespace TP08A.Contracts;

public interface IResourceUtilizationService
{
    Task<OperationResult<ReportResult>> BuildAsync(
        IReadOnlyList<ReportRow> source,
        ReportQuery query,
        CancellationToken cancellationToken = default);
}
