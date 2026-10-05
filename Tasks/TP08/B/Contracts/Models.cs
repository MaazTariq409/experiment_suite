namespace TP08B.Contracts;

public sealed record ReportRow(string ResourceId, DateOnly Start, DateOnly End, decimal Units);

public sealed record ReportQuery(DateOnly From, DateOnly To);

public sealed record ReportLine(string ResourceId, decimal TotalUnits, decimal UtilizationPercent);

public sealed record ReportResult(IReadOnlyList<ReportLine> Lines, decimal OverallUtilizationPercent);

/// <summary>
/// Frozen reporting rules for Support Ticket SLA (TP08-B).
/// Same aggregation math as TP08-A; ResourceId represents a team, Units a daily SLA credit rate.
/// </summary>
public static class SlaRules
{
    public static int InclusiveDays(DateOnly start, DateOnly end) =>
        end.DayNumber - start.DayNumber + 1;

    public static int OverlapDays(DateOnly start, DateOnly end, DateOnly from, DateOnly to)
    {
        var overlapStart = start > from ? start : from;
        var overlapEnd = end < to ? end : to;
        if (overlapEnd < overlapStart)
            return 0;
        return InclusiveDays(overlapStart, overlapEnd);
    }

    public static decimal Percent(decimal totalUnits, int capacityDays) =>
        capacityDays <= 0
            ? 0m
            : Math.Round(totalUnits / capacityDays * 100m, 2, MidpointRounding.AwayFromZero);
}
