namespace TP03A.Contracts;

/// <summary>
/// FROZEN models for Invoice Aging (TP03-A).
/// Outstanding = Amount - PaidAmount. Rows with outstanding &lt;= 0 are excluded from totals.
/// </summary>
public sealed record InvoiceRow(
    string Id,
    string OwnerKey,
    DateOnly DueDate,
    decimal Amount,
    decimal PaidAmount);

public sealed record AgingBucket(string Name, decimal TotalAmount, int Count);

public sealed record AgingReport(IReadOnlyList<AgingBucket> Buckets, decimal GrandTotal);

public sealed record AgingQuery(string? OwnerKey, DateOnly AsOfDate);

/// <summary>Frozen bucket names and order.</summary>
public static class AgingBuckets
{
    public const string Current = "Current";
    public const string D1To30 = "1-30";
    public const string D31To60 = "31-60";
    public const string D61To90 = "61-90";
    public const string D90Plus = "90+";

    public static readonly string[] OrderedNames =
    [
        Current, D1To30, D31To60, D61To90, D90Plus
    ];

    public static string Classify(int daysPastDue) => daysPastDue switch
    {
        <= 0 => Current,
        <= 30 => D1To30,
        <= 60 => D31To60,
        <= 90 => D61To90,
        _ => D90Plus
    };
}
