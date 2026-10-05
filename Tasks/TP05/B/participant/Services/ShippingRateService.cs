using Enterprise.Shared.Results;
using TP05B.Contracts;

namespace TP05B.Participant.Services;

/// <summary>PARTICIPANT IMPLEMENTATION AREA.</summary>
public sealed class ShippingRateService : IShippingRateService
{
    private readonly IExternalQuoteClient _client;

    public ShippingRateService(IExternalQuoteClient client) => _client = client;

    public Task<OperationResult<QuoteResponse>> GetQuoteAsync(
        ExternalQuoteRequest request,
        CancellationToken cancellationToken = default)
    {
        // TODO: validate request; call _client; map timeout/failure/rate; compute ConvertedAmount = Amount * Rate
        throw new NotImplementedException("Implement external mapping/error handling for TP05-B.");
    }
}
