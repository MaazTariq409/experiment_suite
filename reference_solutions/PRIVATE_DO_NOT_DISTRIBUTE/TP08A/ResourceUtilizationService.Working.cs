using Enterprise.Shared.Results;
using TP08A.Contracts;

namespace TP08A.Participant.Services;

/// <summary>REFERENCE IMPLEMENTATION — private oracle.</summary>
public sealed class ResourceUtilizationService : IResourceUtilizationService
{
    public Task<OperationResult<ReportResult>> BuildAsync(
        IReadOnlyList<ReportRow> source,
        ReportQuery query,
        CancellationToken cancellationToken = default)
    {
        if (source is null)
            return Task.FromResult(OperationResult<ReportResult>.Fail("VALIDATION_ERROR", "Source is required.", 400));

        if (query.From > query.To)
            return Task.FromResult(OperationResult<ReportResult>.Fail("VALIDATION_ERROR", "Invalid date range.", 400));

        foreach (var row in source)
        {
            if (string.IsNullOrWhiteSpace(row.ResourceId) || row.End < row.Start || row.Units < 0)
                return Task.FromResult(OperationResult<ReportResult>.Fail("VALIDATION_ERROR", "Invalid row.", 400));
        }

        var capacity = UtilizationRules.InclusiveDays(query.From, query.To);
        var totals = new Dictionary<string, decimal>(StringComparer.Ordinal);

        foreach (var row in source)
        {
            var id = row.ResourceId.Trim();
            var overlap = UtilizationRules.OverlapDays(row.Start, row.End, query.From, query.To);
            if (overlap <= 0 || row.Units == 0)
                continue;

            var contribution = row.Units * overlap;
            totals[id] = totals.TryGetValue(id, out var existing) ? existing + contribution : contribution;
        }

        var lines = totals
            .OrderBy(kv => kv.Key, StringComparer.Ordinal)
            .Select(kv => new ReportLine(kv.Key, kv.Value, UtilizationRules.Percent(kv.Value, capacity)))
            .ToList();

        var grand = totals.Values.Sum();
        var result = new ReportResult(lines, UtilizationRules.Percent(grand, capacity));
        return Task.FromResult(OperationResult<ReportResult>.Ok(result));
    }
}
