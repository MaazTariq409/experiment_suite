using Enterprise.Shared.Results;
using TP04B.Contracts;

namespace TP04B.Participant.Services;

/// <summary>REFERENCE IMPLEMENTATION — private oracle.</summary>
public sealed class ExpenseReimbursementService : IExpenseReimbursementService
{
    public Task<OperationResult<TransitionResult>> TransitionAsync(
        TransitionRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Id == Guid.Empty ||
            !ExpenseWorkflow.Statuses.Contains(request.CurrentStatus) ||
            !ExpenseWorkflow.Statuses.Contains(request.TargetStatus) ||
            !ExpenseWorkflow.Roles.Contains(request.ActorRole))
        {
            return Task.FromResult(OperationResult<TransitionResult>.Fail("VALIDATION_ERROR", "Invalid request.", 400));
        }

        var from = ExpenseWorkflow.Canonical(request.CurrentStatus);
        var to = ExpenseWorkflow.Canonical(request.TargetStatus);
        var role = request.ActorRole;

        if (ExpenseWorkflow.Terminal.Contains(from) || !IsEdgeAllowed(from, to))
        {
            return Ok(from, applied: false, "INVALID_TRANSITION");
        }

        if (!IsRoleAllowed(from, to, role))
        {
            return Ok(from, applied: false, "FORBIDDEN");
        }

        if ((to is "Approved" or "Rejected") && string.IsNullOrWhiteSpace(request.Comment))
        {
            return Ok(from, applied: false, "COMMENT_REQUIRED");
        }

        return Ok(to, applied: true, "APPLIED");
    }

    private static bool IsEdgeAllowed(string from, string to) => (from, to) switch
    {
        ("Draft", "Submitted") => true,
        ("Draft", "Cancelled") => true,
        ("Submitted", "Approved") => true,
        ("Submitted", "Rejected") => true,
        ("Submitted", "Cancelled") => true,
        _ => false
    };

    private static bool IsRoleAllowed(string from, string to, string role)
    {
        var isManagerOrAdmin =
            string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);

        return (from, to) switch
        {
            ("Draft", "Submitted") => true,
            ("Draft", "Cancelled") => true,
            ("Submitted", "Cancelled") => true,
            ("Submitted", "Approved") => isManagerOrAdmin,
            ("Submitted", "Rejected") => isManagerOrAdmin,
            _ => false
        };
    }

    private static Task<OperationResult<TransitionResult>> Ok(string status, bool applied, string reason) =>
        Task.FromResult(OperationResult<TransitionResult>.Ok(new TransitionResult(status, applied, reason)));
}
