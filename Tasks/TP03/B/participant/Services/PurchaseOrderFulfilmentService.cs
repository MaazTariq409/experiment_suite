using Enterprise.Shared.Results;
using TP03B.Contracts;

namespace TP03B.Participant.Services;

/// <summary>
/// PARTICIPANT IMPLEMENTATION AREA.
/// </summary>
public sealed class PurchaseOrderFulfilmentService : IPurchaseOrderFulfilmentService
{
    public Task<OperationResult<AgingReport>> BuildReportAsync(
        IReadOnlyList<PurchaseOrderRow> source,
        AgingQuery query,
        CancellationToken cancellationToken = default)
    {
        // TODO:
        // - reject null source (400 VALIDATION_ERROR)
        // - reject OrderedAmount < 0, ReceivedAmount < 0, or ReceivedAmount > OrderedAmount
        // - filter by OwnerKey when provided (case-insensitive)
        // - outstanding = OrderedAmount - ReceivedAmount; exclude outstanding <= 0
        // - classify by daysPastDue = AsOfDate.DayNumber - DueDate.DayNumber
        // - always return 5 buckets in AgingBuckets.OrderedNames order
        throw new NotImplementedException("Implement aggregation for TP03-B.");
    }
}
