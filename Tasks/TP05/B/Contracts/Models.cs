namespace TP05B.Contracts;

public sealed record ExternalQuoteRequest(string From, string To, decimal Amount);

public sealed record ExternalQuoteRaw(string From, string To, decimal Rate, bool TimedOut, bool Failed);

public sealed record QuoteResponse(
    string From,
    string To,
    decimal Rate,
    decimal ConvertedAmount,
    string Source);

public interface IExternalQuoteClient
{
    Task<ExternalQuoteRaw> GetQuoteAsync(
        ExternalQuoteRequest request,
        CancellationToken cancellationToken = default);
}

public static class ShippingQuoteRules
{
    public const string ExternalSource = "External";
    public const string GatewayTimeout = "GATEWAY_TIMEOUT";
    public const string BadGateway = "BAD_GATEWAY";

    public static bool IsValidCode(string? code) =>
        !string.IsNullOrWhiteSpace(code)
        && code.Trim().Length == 3
        && code.Trim().All(char.IsLetter);
}
