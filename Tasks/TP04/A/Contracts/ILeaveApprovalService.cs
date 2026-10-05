using Enterprise.Shared.Results;

namespace TP04A.Contracts;

public interface ILeaveApprovalService
{
    Task<OperationResult<TransitionResult>> TransitionAsync(
        TransitionRequest request,
        CancellationToken cancellationToken = default);
}
