using Enterprise.Shared.Results;
using TP05A.Contracts;

namespace TP05A.Participant.Services;

/// <summary>PARTICIPANT IMPLEMENTATION AREA.</summary>
public sealed class CurrencyRateService : ICurrencyRateService
{
    private readonly IExternalQuoteClient _client;

    public CurrencyRateService(IExternalQuoteClient client) => _client = client;

    public Task<OperationResult<QuoteResponse>> GetQuoteAsync(
        ExternalQuoteRequest request,
        CancellationToken cancellationToken = default)
    {
        // TODO: validate request; call _client; map timeout/failure/rate; compute ConvertedAmount = Amount * Rate
        throw new NotImplementedException("Implement external mapping/error handling for TP05-A.");
    }
}
