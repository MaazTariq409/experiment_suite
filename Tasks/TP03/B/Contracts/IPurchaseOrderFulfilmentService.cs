using Enterprise.Shared.Results;

namespace TP03B.Contracts;

/// <summary>
/// FROZEN CONTRACT — implement purchase-order fulfilment aging; do not rename members.
/// </summary>
public interface IPurchaseOrderFulfilmentService
{
    Task<OperationResult<AgingReport>> BuildReportAsync(
        IReadOnlyList<PurchaseOrderRow> source,
        AgingQuery query,
        CancellationToken cancellationToken = default);
}
