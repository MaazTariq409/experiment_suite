namespace TP06A.Contracts;

public sealed record ImportRow(int LineNumber, string Raw);

public sealed record ImportSummary(int Accepted, int Rejected, IReadOnlyList<string> Errors);

public sealed record EmployeeRecord(string Key, string Name, decimal Value);

/// <summary>Frozen CSV import rules for Employee Import (TP06-A).</summary>
public static class EmployeeImportRules
{
    public const string Malformed = "MALFORMED";
    public const string EmptyKey = "EMPTY_KEY";
    public const string EmptyName = "EMPTY_NAME";
    public const string InvalidValue = "INVALID_VALUE";
    public const string DuplicateKey = "DUPLICATE_KEY";

    public static string FormatError(int lineNumber, string reason) => $"Line {lineNumber}: {reason}";
}
