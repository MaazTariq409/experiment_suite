namespace TP01A.Contracts;

public sealed class MaintenanceRecordCreateRequest
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateOnly ScheduledDate { get; set; }
    public decimal EstimatedCost { get; set; }
    public string Status { get; set; } = "Draft";
}

public sealed class MaintenanceRecordUpdateRequest
{
    public string Title { get; set; } = string.Empty;
    public DateOnly ScheduledDate { get; set; }
    public decimal EstimatedCost { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class MaintenanceRecordResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateOnly ScheduledDate { get; set; }
    public decimal EstimatedCost { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }
}
