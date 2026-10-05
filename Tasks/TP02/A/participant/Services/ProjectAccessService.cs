using Enterprise.Shared.Results;
using TP02A.Contracts;

namespace TP02A.Participant.Services;

/// <summary>
/// PARTICIPANT IMPLEMENTATION AREA.
/// Implement the Project Access Policy matrix from TASK_SPEC.md.
/// </summary>
public sealed class ProjectAccessService : IProjectAccessService
{
    public Task<OperationResult<AccessDecision>> AuthorizeAsync(
        AccessRequest request,
        CancellationToken cancellationToken = default)
    {
        // TODO:
        // 1) Validate ActorUserId, ResourceId non-whitespace
        // 2) Validate Role/Action/Status against ProjectAccessVocabulary
        // 3) Apply Closed + mutation => INVALID_STATE
        // 4) Apply role/action matrix; deny with FORBIDDEN
        // 5) Allow with ReasonCode ALLOWED
        throw new NotImplementedException("Implement authorization rules for TP02-A.");
    }
}
