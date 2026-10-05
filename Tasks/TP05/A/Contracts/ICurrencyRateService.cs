using Enterprise.Shared.Results;

namespace TP05A.Contracts;

public interface ICurrencyRateService
{
    Task<OperationResult<QuoteResponse>> GetQuoteAsync(
        ExternalQuoteRequest request,
        CancellationToken cancellationToken = default);
}
