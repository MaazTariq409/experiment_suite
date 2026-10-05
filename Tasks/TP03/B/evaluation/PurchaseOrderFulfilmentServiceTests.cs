using FluentAssertions;
using TP03B.Contracts;
using TP03B.Participant.Services;
using Xunit;

namespace TP03B.Evaluation;

/// <summary>HIDDEN EVALUATION — 20 tests for TP03-B Purchase Order Fulfilment.</summary>
public class PurchaseOrderFulfilmentServiceTests
{
    private static readonly DateOnly AsOf = new(2026, 3, 15);
    private static IPurchaseOrderFulfilmentService Sut() => new PurchaseOrderFulfilmentService();

    private static List<PurchaseOrderRow> Sample() =>
    [
        new("1", "ACME", new DateOnly(2026, 1, 1), 100m, 0m),
        new("2", "ACME", new DateOnly(2026, 3, 1), 50m, 10m),
        new("3", "BETA", new DateOnly(2026, 3, 10), 20m, 20m)
    ];

    [Fact]
    public async Task NullSource_ReturnsValidationError()
    {
        var result = await Sut().BuildReportAsync(null!, new AgingQuery(null, AsOf));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public async Task NegativeOrderedAmount_ReturnsValidationError()
    {
        var rows = new[] { new PurchaseOrderRow("1", "ACME", AsOf, -1m, 0m) };
        var result = await Sut().BuildReportAsync(rows, new AgingQuery(null, AsOf));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task ReceivedExceedsOrdered_ReturnsValidationError()
    {
        var rows = new[] { new PurchaseOrderRow("1", "ACME", AsOf, 10m, 11m) };
        var result = await Sut().BuildReportAsync(rows, new AgingQuery(null, AsOf));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task NegativeReceivedAmount_ReturnsValidationError()
    {
        var rows = new[] { new PurchaseOrderRow("1", "ACME", AsOf, 10m, -1m) };
        var result = await Sut().BuildReportAsync(rows, new AgingQuery(null, AsOf));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task EmptySource_ReturnsFiveZeroBuckets()
    {
        var result = await Sut().BuildReportAsync([], new AgingQuery(null, AsOf));
        result.Succeeded.Should().BeTrue();
        result.Value!.GrandTotal.Should().Be(0m);
        result.Value.Buckets.Select(b => b.Name).Should().Equal(AgingBuckets.OrderedNames);
        result.Value.Buckets.Should().OnlyContain(b => b.TotalAmount == 0m && b.Count == 0);
    }

    [Fact]
    public async Task OwnerFilter_AppliesCaseInsensitive()
    {
        var result = await Sut().BuildReportAsync(Sample(), new AgingQuery("acme", AsOf));
        result.Succeeded.Should().BeTrue();
        result.Value!.GrandTotal.Should().Be(140m);
    }

    [Fact]
    public async Task NullOwner_IncludesAllOwners()
    {
        var rows = new List<PurchaseOrderRow>(Sample())
        {
            new("4", "GAMMA", new DateOnly(2026, 3, 10), 30m, 0m)
        };
        var result = await Sut().BuildReportAsync(rows, new AgingQuery(null, AsOf));
        result.Succeeded.Should().BeTrue();
        result.Value!.GrandTotal.Should().Be(170m);
    }

    [Fact]
    public async Task ZeroOutstanding_ExcludedFromGrandTotal()
    {
        var result = await Sut().BuildReportAsync(Sample(), new AgingQuery("BETA", AsOf));
        result.Succeeded.Should().BeTrue();
        result.Value!.GrandTotal.Should().Be(0m);
        result.Value.Buckets.Sum(b => b.Count).Should().Be(0);
    }

    [Fact]
    public async Task BuildsDeterministicGrandTotal_ForAcme()
    {
        var result = await Sut().BuildReportAsync(Sample(), new AgingQuery("ACME", AsOf));
        result.Succeeded.Should().BeTrue();
        result.Value!.GrandTotal.Should().Be(140m);
    }

    [Fact]
    public async Task BucketOrder_IsFrozen()
    {
        var result = await Sut().BuildReportAsync(Sample(), new AgingQuery("ACME", AsOf));
        result.Value!.Buckets.Select(b => b.Name).Should().Equal(AgingBuckets.OrderedNames);
    }

    [Fact]
    public async Task Classifies_Current_WhenNotYetDue()
    {
        var rows = new[] { new PurchaseOrderRow("1", "ACME", new DateOnly(2026, 4, 1), 25m, 0m) };
        var result = await Sut().BuildReportAsync(rows, new AgingQuery(null, AsOf));
        result.Value!.Buckets.Single(b => b.Name == AgingBuckets.Current).TotalAmount.Should().Be(25m);
        result.Value.Buckets.Single(b => b.Name == AgingBuckets.Current).Count.Should().Be(1);
    }

    [Fact]
    public async Task Boundary_Day0_IsCurrent()
    {
        var rows = new[] { new PurchaseOrderRow("1", "ACME", AsOf, 10m, 0m) };
        var result = await Sut().BuildReportAsync(rows, new AgingQuery(null, AsOf));
        result.Value!.Buckets.Single(b => b.Name == AgingBuckets.Current).Count.Should().Be(1);
    }

    [Fact]
    public async Task Boundary_Day1_Is1To30()
    {
        var rows = new[] { new PurchaseOrderRow("1", "ACME", AsOf.AddDays(-1), 10m, 0m) };
        var result = await Sut().BuildReportAsync(rows, new AgingQuery(null, AsOf));
        result.Value!.Buckets.Single(b => b.Name == AgingBuckets.D1To30).TotalAmount.Should().Be(10m);
    }

    [Fact]
    public async Task Boundary_Day30_Is1To30()
    {
        var rows = new[] { new PurchaseOrderRow("1", "ACME", AsOf.AddDays(-30), 10m, 0m) };
        var result = await Sut().BuildReportAsync(rows, new AgingQuery(null, AsOf));
        result.Value!.Buckets.Single(b => b.Name == AgingBuckets.D1To30).Count.Should().Be(1);
    }

    [Fact]
    public async Task Boundary_Day31_Is31To60()
    {
        var rows = new[] { new PurchaseOrderRow("1", "ACME", AsOf.AddDays(-31), 10m, 0m) };
        var result = await Sut().BuildReportAsync(rows, new AgingQuery(null, AsOf));
        result.Value!.Buckets.Single(b => b.Name == AgingBuckets.D31To60).Count.Should().Be(1);
    }

    [Fact]
    public async Task Boundary_Day60_Is31To60()
    {
        var rows = new[] { new PurchaseOrderRow("1", "ACME", AsOf.AddDays(-60), 10m, 0m) };
        var result = await Sut().BuildReportAsync(rows, new AgingQuery(null, AsOf));
        result.Value!.Buckets.Single(b => b.Name == AgingBuckets.D31To60).Count.Should().Be(1);
    }

    [Fact]
    public async Task Boundary_Day61_Is61To90()
    {
        var rows = new[] { new PurchaseOrderRow("1", "ACME", AsOf.AddDays(-61), 10m, 0m) };
        var result = await Sut().BuildReportAsync(rows, new AgingQuery(null, AsOf));
        result.Value!.Buckets.Single(b => b.Name == AgingBuckets.D61To90).Count.Should().Be(1);
    }

    [Fact]
    public async Task Boundary_Day90_Is61To90()
    {
        var rows = new[] { new PurchaseOrderRow("1", "ACME", AsOf.AddDays(-90), 10m, 0m) };
        var result = await Sut().BuildReportAsync(rows, new AgingQuery(null, AsOf));
        result.Value!.Buckets.Single(b => b.Name == AgingBuckets.D61To90).Count.Should().Be(1);
    }

    [Fact]
    public async Task Boundary_Day91_Is90Plus()
    {
        var rows = new[] { new PurchaseOrderRow("1", "ACME", AsOf.AddDays(-91), 10m, 0m) };
        var result = await Sut().BuildReportAsync(rows, new AgingQuery(null, AsOf));
        result.Value!.Buckets.Single(b => b.Name == AgingBuckets.D90Plus).Count.Should().Be(1);
    }

    [Fact]
    public async Task AggregatesMultipleRows_InSameBucket()
    {
        var rows = new[]
        {
            new PurchaseOrderRow("1", "ACME", AsOf.AddDays(-5), 10m, 0m),
            new PurchaseOrderRow("2", "ACME", AsOf.AddDays(-8), 15m, 5m)
        };
        var result = await Sut().BuildReportAsync(rows, new AgingQuery(null, AsOf));
        var bucket = result.Value!.Buckets.Single(b => b.Name == AgingBuckets.D1To30);
        bucket.Count.Should().Be(2);
        bucket.TotalAmount.Should().Be(20m);
        result.Value.GrandTotal.Should().Be(20m);
    }
}
