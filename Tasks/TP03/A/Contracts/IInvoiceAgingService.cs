using Enterprise.Shared.Results;

namespace TP03A.Contracts;

/// <summary>
/// FROZEN CONTRACT — implement invoice aging aggregation; do not rename members.
/// </summary>
public interface IInvoiceAgingService
{
    Task<OperationResult<AgingReport>> BuildReportAsync(
        IReadOnlyList<InvoiceRow> source,
        AgingQuery query,
        CancellationToken cancellationToken = default);
}
