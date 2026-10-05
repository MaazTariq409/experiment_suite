using Enterprise.Shared.Results;
using TP03A.Contracts;

namespace TP03A.Participant.Services;

/// <summary>REFERENCE IMPLEMENTATION — private oracle for evaluator verification.</summary>
public sealed class InvoiceAgingService : IInvoiceAgingService
{
    public Task<OperationResult<AgingReport>> BuildReportAsync(
        IReadOnlyList<InvoiceRow> source,
        AgingQuery query,
        CancellationToken cancellationToken = default)
    {
        if (source is null)
            return Task.FromResult(OperationResult<AgingReport>.Fail("VALIDATION_ERROR", "Source is required.", 400));

        foreach (var row in source)
        {
            if (row.Amount < 0 || row.PaidAmount < 0 || row.PaidAmount > row.Amount)
                return Task.FromResult(OperationResult<AgingReport>.Fail("VALIDATION_ERROR", "Invalid amounts.", 400));
        }

        var filtered = source.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(query.OwnerKey))
            filtered = filtered.Where(r => string.Equals(r.OwnerKey, query.OwnerKey, StringComparison.OrdinalIgnoreCase));

        var totals = AgingBuckets.OrderedNames.ToDictionary(n => n, _ => (Total: 0m, Count: 0));

        foreach (var row in filtered)
        {
            var outstanding = row.Amount - row.PaidAmount;
            if (outstanding <= 0)
                continue;

            var days = query.AsOfDate.DayNumber - row.DueDate.DayNumber;
            var bucket = AgingBuckets.Classify(days);
            var current = totals[bucket];
            totals[bucket] = (current.Total + outstanding, current.Count + 1);
        }

        var buckets = AgingBuckets.OrderedNames
            .Select(n => new AgingBucket(n, totals[n].Total, totals[n].Count))
            .ToList();

        var grand = buckets.Sum(b => b.TotalAmount);
        return Task.FromResult(OperationResult<AgingReport>.Ok(new AgingReport(buckets, grand)));
    }
}
