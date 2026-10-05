namespace TP01B.Contracts;

public sealed class SupplierContractCreateRequest
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateOnly ScheduledDate { get; set; }
    public decimal EstimatedCost { get; set; }
    public string Status { get; set; } = "Draft";
}

public sealed class SupplierContractUpdateRequest
{
    public string Title { get; set; } = string.Empty;
    public DateOnly ScheduledDate { get; set; }
    public decimal EstimatedCost { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class SupplierContractResponse
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
