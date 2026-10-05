using Enterprise.Shared.Results;

namespace TP02B.Contracts;

/// <summary>
/// FROZEN CONTRACT — participants implement this interface; do not rename members.
/// </summary>
public interface IDocumentAccessService
{
    Task<OperationResult<AccessDecision>> AuthorizeAsync(
        AccessRequest request,
        CancellationToken cancellationToken = default);
}
