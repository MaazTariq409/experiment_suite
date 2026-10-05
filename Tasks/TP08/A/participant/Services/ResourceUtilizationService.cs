using Enterprise.Shared.Results;
using TP08A.Contracts;

namespace TP08A.Participant.Services;

/// <summary>PARTICIPANT IMPLEMENTATION AREA.</summary>
public sealed class ResourceUtilizationService : IResourceUtilizationService
{
    public Task<OperationResult<ReportResult>> BuildAsync(
        IReadOnlyList<ReportRow> source,
        ReportQuery query,
        CancellationToken cancellationToken = default)
    {
        // TODO: validate range/rows; clip overlaps; aggregate by ResourceId; compute utilization %
        throw new NotImplementedException("Implement reporting/date logic for TP08-A.");
    }
}
