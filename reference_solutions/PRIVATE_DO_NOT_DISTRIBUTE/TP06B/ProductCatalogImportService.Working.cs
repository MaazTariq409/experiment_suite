using System.Globalization;
using Enterprise.Shared.Results;
using TP06B.Contracts;

namespace TP06B.Participant.Services;

/// <summary>REFERENCE IMPLEMENTATION — private oracle.</summary>
public sealed class ProductCatalogImportService : IProductCatalogImportService
{
    public Task<OperationResult<ImportSummary>> ImportAsync(
        IReadOnlyList<ImportRow> rows,
        CancellationToken cancellationToken = default)
    {
        if (rows is null)
            return Task.FromResult(OperationResult<ImportSummary>.Fail("VALIDATION_ERROR", "Rows are required.", 400));

        var accepted = 0;
        var rejected = 0;
        var errors = new List<(int Line, string Message)>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows.OrderBy(r => r.LineNumber))
        {
            if (row.LineNumber <= 0 || string.IsNullOrWhiteSpace(row.Raw))
            {
                rejected++;
                errors.Add((row.LineNumber, ProductImportRules.FormatError(row.LineNumber, ProductImportRules.Malformed)));
                continue;
            }

            var parts = row.Raw.Split(',');
            if (parts.Length != 3)
            {
                rejected++;
                errors.Add((row.LineNumber, ProductImportRules.FormatError(row.LineNumber, ProductImportRules.Malformed)));
                continue;
            }

            var key = parts[0].Trim();
            var name = parts[1].Trim();
            var valueRaw = parts[2].Trim();

            if (string.IsNullOrWhiteSpace(key))
            {
                rejected++;
                errors.Add((row.LineNumber, ProductImportRules.FormatError(row.LineNumber, ProductImportRules.EmptyKey)));
                continue;
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                rejected++;
                errors.Add((row.LineNumber, ProductImportRules.FormatError(row.LineNumber, ProductImportRules.EmptyName)));
                continue;
            }

            if (!decimal.TryParse(valueRaw, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) || value < 0)
            {
                rejected++;
                errors.Add((row.LineNumber, ProductImportRules.FormatError(row.LineNumber, ProductImportRules.InvalidValue)));
                continue;
            }

            if (!seen.Add(key))
            {
                rejected++;
                errors.Add((row.LineNumber, ProductImportRules.FormatError(row.LineNumber, ProductImportRules.DuplicateKey)));
                continue;
            }

            accepted++;
        }

        var summary = new ImportSummary(
            accepted,
            rejected,
            errors.OrderBy(e => e.Line).Select(e => e.Message).ToList());

        return Task.FromResult(OperationResult<ImportSummary>.Ok(summary));
    }
}
