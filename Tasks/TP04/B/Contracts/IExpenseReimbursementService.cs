using Enterprise.Shared.Results;

namespace TP04B.Contracts;

public interface IExpenseReimbursementService
{
    Task<OperationResult<TransitionResult>> TransitionAsync(
        TransitionRequest request,
        CancellationToken cancellationToken = default);
}
