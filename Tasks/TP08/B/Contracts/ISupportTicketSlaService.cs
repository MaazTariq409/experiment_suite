using Enterprise.Shared.Results;

namespace TP08B.Contracts;

public interface ISupportTicketSlaService
{
    Task<OperationResult<ReportResult>> BuildAsync(
        IReadOnlyList<ReportRow> source,
        ReportQuery query,
        CancellationToken cancellationToken = default);
}
