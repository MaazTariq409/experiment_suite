using Enterprise.Shared.Results;
using TP05A.Contracts;

namespace TP05A.Participant.Services;

/// <summary>REFERENCE IMPLEMENTATION — private oracle.</summary>
public sealed class CurrencyRateService : ICurrencyRateService
{
    private readonly IExternalQuoteClient _client;

    public CurrencyRateService(IExternalQuoteClient client) => _client = client;

    public async Task<OperationResult<QuoteResponse>> GetQuoteAsync(
        ExternalQuoteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return OperationResult<QuoteResponse>.Fail("VALIDATION_ERROR", "Request is required.", 400);

        var from = request.From?.Trim() ?? string.Empty;
        var to = request.To?.Trim() ?? string.Empty;

        if (!CurrencyQuoteRules.IsValidCode(from) ||
            !CurrencyQuoteRules.IsValidCode(to) ||
            string.Equals(from, to, StringComparison.OrdinalIgnoreCase) ||
            request.Amount <= 0)
        {
            return OperationResult<QuoteResponse>.Fail("VALIDATION_ERROR", "Invalid quote request.", 400);
        }

        from = from.ToUpperInvariant();
        to = to.ToUpperInvariant();

        var raw = await _client.GetQuoteAsync(new ExternalQuoteRequest(from, to, request.Amount), cancellationToken);

        if (raw.TimedOut)
            return OperationResult<QuoteResponse>.Fail(CurrencyQuoteRules.GatewayTimeout, "External service timed out.", 504);

        if (raw.Failed || raw.Rate <= 0)
            return OperationResult<QuoteResponse>.Fail(CurrencyQuoteRules.BadGateway, "External service failed.", 502);

        var response = new QuoteResponse(
            from,
            to,
            raw.Rate,
            request.Amount * raw.Rate,
            CurrencyQuoteRules.ExternalSource);

        return OperationResult<QuoteResponse>.Ok(response);
    }
}
