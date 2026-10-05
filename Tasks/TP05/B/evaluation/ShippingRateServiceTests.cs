using FluentAssertions;
using TP05B.Contracts;
using TP05B.Participant.Services;
using Xunit;

namespace TP05B.Evaluation;

sealed class FakeClient(ExternalQuoteRaw raw) : IExternalQuoteClient
{
    public ExternalQuoteRequest? LastRequest { get; private set; }

    public Task<ExternalQuoteRaw> GetQuoteAsync(
        ExternalQuoteRequest request,
        CancellationToken cancellationToken = default)
    {
        LastRequest = request;
        return Task.FromResult(raw);
    }
}

/// <summary>HIDDEN EVALUATION — 20 tests for TP05-B Shipping Rate Service.</summary>
public class ShippingRateServiceTests
{
    private static (ShippingRateService Sut, FakeClient Client) Create(
        string from = "LAX",
        string to = "JFK",
        decimal rate = 0.9m,
        bool timedOut = false,
        bool failed = false)
    {
        var client = new FakeClient(new ExternalQuoteRaw(from, to, rate, timedOut, failed));
        return (new ShippingRateService(client), client);
    }

    [Fact]
    public async Task NullRequest_ReturnsValidationError()
    {
        var (sut, _) = Create();
        var result = await sut.GetQuoteAsync(null!);
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public async Task BlankFrom_ReturnsValidationError()
    {
        var (sut, _) = Create();
        var result = await sut.GetQuoteAsync(new ExternalQuoteRequest(" ", "JFK", 100m));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task BlankTo_ReturnsValidationError()
    {
        var (sut, _) = Create();
        var result = await sut.GetQuoteAsync(new ExternalQuoteRequest("LAX", "", 100m));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task InvalidCodeLength_ReturnsValidationError()
    {
        var (sut, _) = Create();
        var result = await sut.GetQuoteAsync(new ExternalQuoteRequest("LA", "JFK", 100m));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task NonLetterCode_ReturnsValidationError()
    {
        var (sut, _) = Create();
        var result = await sut.GetQuoteAsync(new ExternalQuoteRequest("LA1", "JFK", 100m));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task SameFromAndTo_ReturnsValidationError()
    {
        var (sut, _) = Create();
        var result = await sut.GetQuoteAsync(new ExternalQuoteRequest("lax", "LAX", 100m));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task ZeroAmount_ReturnsValidationError()
    {
        var (sut, _) = Create();
        var result = await sut.GetQuoteAsync(new ExternalQuoteRequest("LAX", "JFK", 0m));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task NegativeAmount_ReturnsValidationError()
    {
        var (sut, _) = Create();
        var result = await sut.GetQuoteAsync(new ExternalQuoteRequest("LAX", "JFK", -5m));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task MapsSuccessfulExternalResponse()
    {
        var (sut, _) = Create(rate: 0.9m);
        var result = await sut.GetQuoteAsync(new ExternalQuoteRequest("LAX", "JFK", 100m));
        result.Succeeded.Should().BeTrue();
        result.Value!.ConvertedAmount.Should().Be(90m);
        result.Value.Rate.Should().Be(0.9m);
        result.Value.Source.Should().Be(ShippingQuoteRules.ExternalSource);
    }

    [Fact]
    public async Task NormalizesCodesToUppercase()
    {
        var (sut, _) = Create(rate: 1.1m);
        var result = await sut.GetQuoteAsync(new ExternalQuoteRequest("lax", "jfk", 10m));
        result.Succeeded.Should().BeTrue();
        result.Value!.From.Should().Be("LAX");
        result.Value.To.Should().Be("JFK");
        result.Value.ConvertedAmount.Should().Be(11m);
    }

    [Fact]
    public async Task Timeout_ReturnsGatewayTimeout()
    {
        var (sut, _) = Create(timedOut: true);
        var result = await sut.GetQuoteAsync(new ExternalQuoteRequest("LAX", "JFK", 100m));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(504);
        result.ErrorCode.Should().Be(ShippingQuoteRules.GatewayTimeout);
    }

    [Fact]
    public async Task FailedExternal_ReturnsBadGateway()
    {
        var (sut, _) = Create(failed: true);
        var result = await sut.GetQuoteAsync(new ExternalQuoteRequest("LAX", "JFK", 100m));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(502);
        result.ErrorCode.Should().Be(ShippingQuoteRules.BadGateway);
    }

    [Fact]
    public async Task TimedOut_TakesPrecedenceOverFailed()
    {
        var (sut, _) = Create(timedOut: true, failed: true);
        var result = await sut.GetQuoteAsync(new ExternalQuoteRequest("LAX", "JFK", 100m));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(504);
        result.ErrorCode.Should().Be(ShippingQuoteRules.GatewayTimeout);
    }

    [Fact]
    public async Task ZeroRate_ReturnsBadGateway()
    {
        var (sut, _) = Create(rate: 0m);
        var result = await sut.GetQuoteAsync(new ExternalQuoteRequest("LAX", "JFK", 100m));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(502);
        result.ErrorCode.Should().Be(ShippingQuoteRules.BadGateway);
    }

    [Fact]
    public async Task NegativeRate_ReturnsBadGateway()
    {
        var (sut, _) = Create(rate: -0.5m);
        var result = await sut.GetQuoteAsync(new ExternalQuoteRequest("LAX", "JFK", 100m));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(502);
    }

    [Fact]
    public async Task ForwardsRequestToClient()
    {
        var (sut, client) = Create(rate: 2m);
        var request = new ExternalQuoteRequest("ORD", "MIA", 12.5m);
        await sut.GetQuoteAsync(request);
        client.LastRequest.Should().NotBeNull();
        client.LastRequest!.From.Should().Be("ORD");
        client.LastRequest.To.Should().Be("MIA");
        client.LastRequest.Amount.Should().Be(12.5m);
    }

    [Fact]
    public async Task DoesNotCallClient_WhenValidationFails()
    {
        var (sut, client) = Create();
        await sut.GetQuoteAsync(new ExternalQuoteRequest("LAX", "LAX", 100m));
        client.LastRequest.Should().BeNull();
    }

    [Fact]
    public async Task TrimsCodesBeforeValidation()
    {
        var (sut, _) = Create(rate: 1.25m);
        var result = await sut.GetQuoteAsync(new ExternalQuoteRequest(" LAX ", " jfk ", 8m));
        result.Succeeded.Should().BeTrue();
        result.Value!.From.Should().Be("LAX");
        result.Value.To.Should().Be("JFK");
        result.Value.ConvertedAmount.Should().Be(10m);
    }

    [Fact]
    public async Task LargeAmount_MultipliesExactly()
    {
        var (sut, _) = Create(rate: 1.2345m);
        var result = await sut.GetQuoteAsync(new ExternalQuoteRequest("LAX", "JFK", 1000m));
        result.Succeeded.Should().BeTrue();
        result.Value!.ConvertedAmount.Should().Be(1234.5m);
    }

    [Fact]
    public async Task DifferentPair_Succeeds()
    {
        var (sut, _) = Create(from: "SEA", to: "DEN", rate: 12m);
        var result = await sut.GetQuoteAsync(new ExternalQuoteRequest("SEA", "DEN", 3m));
        result.Succeeded.Should().BeTrue();
        result.Value!.ConvertedAmount.Should().Be(36m);
        result.Value.Source.Should().Be("External");
    }
}
