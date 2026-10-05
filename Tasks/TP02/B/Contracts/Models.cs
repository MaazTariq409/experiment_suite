namespace TP02B.Contracts;

/// <summary>
/// FROZEN request/response models for Document Access Policy (TP02-B).
/// Capability-matched to TP02-A with document-domain vocabulary.
/// </summary>
public sealed record AccessRequest(
    string ActorUserId,
    string ActorRole,
    string ResourceId,
    string Action,
    string ResourceStatus);

public sealed record AccessDecision(bool Allowed, string ReasonCode);

public static class DocumentAccessVocabulary
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
