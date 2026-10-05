using Enterprise.Shared.Results;
using TP03A.Contracts;

namespace TP03A.Participant.Services;

/// <summary>
/// PARTICIPANT IMPLEMENTATION AREA.
/// </summary>
public sealed class InvoiceAgingService : IInvoiceAgingService
{
    public Task<OperationResult<AgingReport>> BuildReportAsync(
        IReadOnlyList<InvoiceRow> source,
        AgingQuery query,
        CancellationToken cancellationToken = default)
    {
        // TODO:
        // - reject null source (400 VALIDATION_ERROR)
        // - reject Amount < 0, PaidAmount < 0, or PaidAmount > Amount
        // - filter by OwnerKey when provided (case-insensitive)
        // - outstanding = Amount - PaidAmount; exclude outstanding <= 0
        // - classify by daysPastDue = AsOfDate.DayNumber - DueDate.DayNumber
        // - always return 5 buckets in AgingBuckets.OrderedNames order
        throw new NotImplementedException("Implement aggregation for TP03-A.");
    }
}
