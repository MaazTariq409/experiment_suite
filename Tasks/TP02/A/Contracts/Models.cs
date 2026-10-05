namespace TP02A.Contracts;

/// <summary>
/// FROZEN request/response models for Project Access Policy (TP02-A).
/// </summary>
public sealed record AccessRequest(
    string ActorUserId,
    string ActorRole,
    string ResourceId,
    string Action,
    string ResourceStatus);

public sealed record AccessDecision(bool Allowed, string ReasonCode);

/// <summary>
/// Canonical vocabulary (frozen). Implementations must accept only these values.
/// </summary>
public static class ProjectAccessVocabulary
{
    public static readonly HashSet<string> Roles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Admin", "Manager", "Contributor", "Viewer"
    };

    public static readonly HashSet<string> Actions = new(StringComparer.OrdinalIgnoreCase)
    {
        "View", "Update", "Approve", "Archive"
    };

    public static readonly HashSet<string> Statuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Open", "InReview", "Closed"
    };

    public static readonly HashSet<string> MutationActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "Update", "Approve", "Archive"
    };
}
