using Enterprise.Shared.Results;
using TP04A.Contracts;

namespace TP04A.Participant.Services;

/// <summary>PARTICIPANT IMPLEMENTATION AREA.</summary>
public sealed class LeaveApprovalService : ILeaveApprovalService
{
    public Task<OperationResult<TransitionResult>> TransitionAsync(
        TransitionRequest request,
        CancellationToken cancellationToken = default)
    {
        // TODO: validate vocabulary; enforce transition graph + role gates;
        // Approve/Reject require non-whitespace Comment; terminal states reject further transitions.
        throw new NotImplementedException("Implement workflow transitions for TP04-A.");
    }
}
