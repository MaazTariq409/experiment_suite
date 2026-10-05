namespace TP06B.Contracts;

public sealed record ImportRow(int LineNumber, string Raw);

public sealed record ImportSummary(int Accepted, int Rejected, IReadOnlyList<string> Errors);

public sealed record ProductRecord(string Key, string Name, decimal Value);

/// <summary>Frozen CSV import rules for Product Catalog Import (TP06-B).</summary>
public static class ProductImportRules
{
    public const string Malformed = "MALFORMED";
    public const string EmptyKey = "EMPTY_KEY";
    public const string EmptyName = "EMPTY_NAME";
    public const string InvalidValue = "INVALID_VALUE";
    public const string DuplicateKey = "DUPLICATE_KEY";

    public static string FormatError(int lineNumber, string reason) => $"Line {lineNumber}: {reason}";
}
