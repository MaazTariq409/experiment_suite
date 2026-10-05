using Enterprise.Shared.Results;

namespace TP05B.Contracts;

public interface IShippingRateService
{
    Task<OperationResult<QuoteResponse>> GetQuoteAsync(
        ExternalQuoteRequest request,
        CancellationToken cancellationToken = default);
}
