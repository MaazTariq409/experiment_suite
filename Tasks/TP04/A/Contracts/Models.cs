namespace TP04A.Contracts;

public sealed record TransitionRequest(
    Guid Id,
    string CurrentStatus,
    string TargetStatus,
    string ActorRole,
    string? Comment);

public sealed record TransitionResult(string NewStatus, bool Applied, string ReasonCode);

/// <summary>Frozen leave-approval vocabulary and transition edges.</summary>
public static class LeaveWorkflow
{
    public static readonly HashSet<string> Statuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Draft", "Submitted", "Approved", "Rejected", "Cancelled"
    };

    public static readonly HashSet<string> Roles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Employee", "Manager", "Admin"
    };

    public static readonly HashSet<string> Terminal = new(StringComparer.OrdinalIgnoreCase)
    {
        "Approved", "Rejected", "Cancelled"
    };

    /// <summary>Canonical casing for successful NewStatus values.</summary>
    public static string Canonical(string status) => status.ToLowerInvariant() switch
    {
        "draft" => "Draft",
        "submitted" => "Submitted",
        "approved" => "Approved",
        "rejected" => "Rejected",
        "cancelled" => "Cancelled",
        _ => status
    };
}
