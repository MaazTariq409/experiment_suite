using Enterprise.Shared.Results;

namespace TP02A.Contracts;

/// <summary>
/// FROZEN CONTRACT — participants implement this interface; do not rename members.
/// Evaluation tests compile only against this contract.
/// </summary>
public interface IProjectAccessService
{
    /// <summary>
    /// Evaluate whether the actor may perform the requested action on the project resource.
    /// Returns OperationResult failure only for malformed/invalid vocabulary inputs.
    /// Authorization denials are successful results with Allowed=false.
    /// </summary>
    Task<OperationResult<AccessDecision>> AuthorizeAsync(
        AccessRequest request,
        CancellationToken cancellationToken = default);
}
