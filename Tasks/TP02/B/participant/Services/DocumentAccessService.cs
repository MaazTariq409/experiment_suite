using Enterprise.Shared.Results;
using TP02B.Contracts;

namespace TP02B.Participant.Services;

/// <summary>
/// PARTICIPANT IMPLEMENTATION AREA.
/// Implement the Document Access Policy matrix from TASK_SPEC.md.
/// </summary>
public sealed class DocumentAccessService : IDocumentAccessService
{
    public Task<OperationResult<AccessDecision>> AuthorizeAsync(
        AccessRequest request,
        CancellationToken cancellationToken = default)
    {
        // TODO:
        // 1) Validate ActorUserId, ResourceId non-whitespace
        // 2) Validate Role/Action/Status against DocumentAccessVocabulary
        // 3) Apply Closed + mutation => INVALID_STATE
        // 4) Apply role/action matrix; deny with FORBIDDEN
        // 5) Allow with ReasonCode ALLOWED
        throw new NotImplementedException("Implement authorization rules for TP02-B.");
    }
}
