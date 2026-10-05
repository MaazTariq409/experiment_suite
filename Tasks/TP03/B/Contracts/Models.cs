namespace TP03B.Contracts;

/// <summary>
/// FROZEN models for Purchase Order Fulfilment (TP03-B).
/// Outstanding = OrderedAmount - ReceivedAmount. Capability-matched to TP03-A.
/// </summary>
public sealed record PurchaseOrderRow(
    string Id,
    string OwnerKey,
    DateOnly DueDate,
    decimal OrderedAmount,
    decimal ReceivedAmount);

public sealed record AgingBucket(string Name, decimal TotalAmount, int Count);

public sealed record AgingReport(IReadOnlyList<AgingBucket> Buckets, decimal GrandTotal);

public sealed record AgingQuery(string? OwnerKey, DateOnly AsOfDate);

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
