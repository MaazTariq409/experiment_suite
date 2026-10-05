using Enterprise.Shared.Results;
using TP02A.Contracts;

namespace TP02A.Participant.Services;

/// <summary>
/// REFERENCE IMPLEMENTATION — private oracle for evaluator verification.
/// </summary>
public sealed class ProjectAccessService : IProjectAccessService
{
    public Task<OperationResult<AccessDecision>> AuthorizeAsync(
        AccessRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.ActorUserId) || string.IsNullOrWhiteSpace(request.ResourceId))
            return Task.FromResult(OperationResult<AccessDecision>.Fail("VALIDATION_ERROR", "UserId and ResourceId are required.", 400));

        if (!ProjectAccessVocabulary.Roles.Contains(request.ActorRole) ||
            !ProjectAccessVocabulary.Actions.Contains(request.Action) ||
            !ProjectAccessVocabulary.Statuses.Contains(request.ResourceStatus))
        {
            return Task.FromResult(OperationResult<AccessDecision>.Fail("VALIDATION_ERROR", "Unknown role, action, or status.", 400));
        }

        var isMutation = ProjectAccessVocabulary.MutationActions.Contains(request.Action);
        if (string.Equals(request.ResourceStatus, "Closed", StringComparison.OrdinalIgnoreCase) && isMutation)
        {
            return Task.FromResult(OperationResult<AccessDecision>.Ok(new AccessDecision(false, "INVALID_STATE")));
        }

        var allowed = IsAllowed(request.ActorRole, request.Action, request.ResourceStatus);
        return Task.FromResult(OperationResult<AccessDecision>.Ok(
            new AccessDecision(allowed, allowed ? "ALLOWED" : "FORBIDDEN")));
    }

    private static bool IsAllowed(string role, string action, string status)
    {
        if (string.Equals(action, "View", StringComparison.OrdinalIgnoreCase))
            return true;

        if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            return true;

        if (string.Equals(role, "Manager", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(action, "Update", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(action, "Approve", StringComparison.OrdinalIgnoreCase);
        }

        if (string.Equals(role, "Contributor", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(action, "Update", StringComparison.OrdinalIgnoreCase)
                   && string.Equals(status, "Open", StringComparison.OrdinalIgnoreCase);
        }

        // Viewer: view-only (already handled)
        return false;
    }
}
