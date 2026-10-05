using FluentAssertions;
using TP08B.Contracts;
using TP08B.Participant.Services;
using Xunit;

namespace TP08B.Evaluation;

/// <summary>HIDDEN EVALUATION — 20 tests for TP08-B Support Ticket SLA.</summary>
public class SupportTicketSlaServiceTests
{
    private static readonly DateOnly From = new(2026, 3, 1);
    private static readonly DateOnly To = new(2026, 3, 31);
    private static ISupportTicketSlaService Sut() => new SupportTicketSlaService();

    [Fact]
    public async Task NullSource_ReturnsValidationError()
    {
        var result = await Sut().BuildAsync(null!, new ReportQuery(From, To));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public async Task InvalidDateRange_ReturnsValidationError()
    {
        var result = await Sut().BuildAsync([], new ReportQuery(new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 1)));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task RowEndBeforeStart_ReturnsValidationError()
    {
        var rows = new[] { new ReportRow("T1", new DateOnly(2026, 3, 5), new DateOnly(2026, 3, 1), 1m) };
        var result = await Sut().BuildAsync(rows, new ReportQuery(From, To));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task BlankResourceId_ReturnsValidationError()
    {
        var rows = new[] { new ReportRow(" ", From, From, 1m) };
        var result = await Sut().BuildAsync(rows, new ReportQuery(From, To));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task NegativeUnits_ReturnsValidationError()
    {
        var rows = new[] { new ReportRow("T1", From, From, -1m) };
        var result = await Sut().BuildAsync(rows, new ReportQuery(From, To));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task EmptyPeriod_ReturnsZeroUtilization()
    {
        var result = await Sut().BuildAsync([], new ReportQuery(From, To));
        result.Succeeded.Should().BeTrue();
        result.Value!.Lines.Should().BeEmpty();
        result.Value.OverallUtilizationPercent.Should().Be(0m);
    }

    [Fact]
    public async Task SingleDayQuery_CapacityIsOne()
    {
        var day = new DateOnly(2026, 3, 15);
        var rows = new[] { new ReportRow("T1", day, day, 1m) };
        var result = await Sut().BuildAsync(rows, new ReportQuery(day, day));
        result.Succeeded.Should().BeTrue();
        result.Value!.Lines.Single().TotalUnits.Should().Be(1m);
        result.Value.Lines.Single().UtilizationPercent.Should().Be(100m);
        result.Value.OverallUtilizationPercent.Should().Be(100m);
    }

    [Fact]
    public async Task FullOverlap_UsesAllAssignmentDays()
    {
        var rows = new[] { new ReportRow("T1", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 5), 1m) };
        var result = await Sut().BuildAsync(rows, new ReportQuery(From, To));
        result.Value!.Lines.Single().TotalUnits.Should().Be(5m);
        result.Value.Lines.Single().UtilizationPercent.Should().Be(SlaRules.Percent(5m, 31));
    }

    [Fact]
    public async Task PartialOverlap_ClipsToQueryWindow()
    {
        var rows = new[] { new ReportRow("T1", new DateOnly(2026, 2, 25), new DateOnly(2026, 3, 3), 2m) };
        var result = await Sut().BuildAsync(rows, new ReportQuery(From, To));
        result.Value!.Lines.Single().TotalUnits.Should().Be(6m);
    }

    [Fact]
    public async Task NoOverlap_OmitsResource()
    {
        var rows = new[] { new ReportRow("T1", new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 5), 1m) };
        var result = await Sut().BuildAsync(rows, new ReportQuery(From, To));
        result.Succeeded.Should().BeTrue();
        result.Value!.Lines.Should().BeEmpty();
        result.Value.OverallUtilizationPercent.Should().Be(0m);
    }

    [Fact]
    public async Task AggregatesByResource()
    {
        var rows = new[]
        {
            new ReportRow("T1", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 5), 1m),
            new ReportRow("T1", new DateOnly(2026, 3, 10), new DateOnly(2026, 3, 12), 1m),
            new ReportRow("T2", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 2), 1m)
        };
        var result = await Sut().BuildAsync(rows, new ReportQuery(From, To));
        result.Succeeded.Should().BeTrue();
        result.Value!.Lines.Should().HaveCount(2);
        result.Value.Lines.Single(l => l.ResourceId == "T1").TotalUnits.Should().Be(8m);
        result.Value.Lines.Single(l => l.ResourceId == "T2").TotalUnits.Should().Be(2m);
        result.Value.OverallUtilizationPercent.Should().Be(SlaRules.Percent(10m, 31));
    }

    [Fact]
    public async Task Lines_AreSortedByResourceId()
    {
        var rows = new[]
        {
            new ReportRow("T2", From, From, 1m),
            new ReportRow("T1", From, From, 1m)
        };
        var result = await Sut().BuildAsync(rows, new ReportQuery(From, To));
        result.Value!.Lines.Select(l => l.ResourceId).Should().Equal("T1", "T2");
    }

    [Fact]
    public async Task ZeroUnits_ContributeNothing_AndOmitResource()
    {
        var rows = new[] { new ReportRow("T1", From, From.AddDays(2), 0m) };
        var result = await Sut().BuildAsync(rows, new ReportQuery(From, To));
        result.Succeeded.Should().BeTrue();
        result.Value!.Lines.Should().BeEmpty();
    }

    [Fact]
    public async Task ResourceId_ComparisonIsOrdinalCaseSensitiveForGrouping()
    {
        var rows = new[]
        {
            new ReportRow("t1", From, From, 1m),
            new ReportRow("T1", From, From, 1m)
        };
        var result = await Sut().BuildAsync(rows, new ReportQuery(From, To));
        result.Value!.Lines.Should().HaveCount(2);
    }

    [Fact]
    public async Task TrimsResourceId()
    {
        var rows = new[] { new ReportRow(" T9 ", From, From, 1m) };
        var result = await Sut().BuildAsync(rows, new ReportQuery(From, To));
        result.Value!.Lines.Single().ResourceId.Should().Be("T9");
    }

    [Fact]
    public async Task SameDayAssignmentAndQuery_Works()
    {
        var rows = new[] { new ReportRow("T1", From, From, 0.5m) };
        var result = await Sut().BuildAsync(rows, new ReportQuery(From, From));
        result.Value!.Lines.Single().TotalUnits.Should().Be(0.5m);
        result.Value.OverallUtilizationPercent.Should().Be(50m);
    }

    [Fact]
    public async Task ClipsEndBeyondQuery()
    {
        var rows = new[] { new ReportRow("T1", new DateOnly(2026, 3, 30), new DateOnly(2026, 4, 5), 1m) };
        var result = await Sut().BuildAsync(rows, new ReportQuery(From, To));
        result.Value!.Lines.Single().TotalUnits.Should().Be(2m);
    }

    [Fact]
    public async Task FractionalUnits_RoundedPercent()
    {
        var rows = new[] { new ReportRow("T1", From, From.AddDays(2), 1.5m) };
        var result = await Sut().BuildAsync(rows, new ReportQuery(From, To));
        result.Value!.Lines.Single().TotalUnits.Should().Be(4.5m);
        result.Value.Lines.Single().UtilizationPercent.Should().Be(SlaRules.Percent(4.5m, 31));
    }

    [Fact]
    public async Task MultipleResources_OverallUsesGrandTotal()
    {
        var rows = new[]
        {
            new ReportRow("A", From, From, 2m),
            new ReportRow("B", From, From, 3m)
        };
        var result = await Sut().BuildAsync(rows, new ReportQuery(From, To));
        result.Value!.OverallUtilizationPercent.Should().Be(SlaRules.Percent(5m, 31));
    }

    [Fact]
    public async Task EqualFromAndTo_IsValid()
    {
        var result = await Sut().BuildAsync([], new ReportQuery(From, From));
        result.Succeeded.Should().BeTrue();
        result.Value!.OverallUtilizationPercent.Should().Be(0m);
    }
}
