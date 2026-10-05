using Enterprise.Shared.Results;
using TP04B.Contracts;

namespace TP04B.Participant.Services;

/// <summary>PARTICIPANT IMPLEMENTATION AREA.</summary>
public sealed class ExpenseReimbursementService : IExpenseReimbursementService
{
    public Task<OperationResult<TransitionResult>> TransitionAsync(
        TransitionRequest request,
        CancellationToken cancellationToken = default)
    {
        // TODO: validate vocabulary; enforce transition graph + role gates;
        // Approve/Reject require non-whitespace Comment; terminal states reject further transitions.
        throw new NotImplementedException("Implement workflow transitions for TP04-B.");
    }
}
