using Enterprise.Shared.Results;
using TP08B.Contracts;

namespace TP08B.Participant.Services;

/// <summary>PARTICIPANT IMPLEMENTATION AREA.</summary>
public sealed class SupportTicketSlaService : ISupportTicketSlaService
{
    public Task<OperationResult<ReportResult>> BuildAsync(
        IReadOnlyList<ReportRow> source,
        ReportQuery query,
        CancellationToken cancellationToken = default)
    {
        // TODO: validate range/rows; clip overlaps; aggregate by ResourceId (team); compute SLA %
        throw new NotImplementedException("Implement reporting/date logic for TP08-B.");
    }
}
