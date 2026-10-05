using Enterprise.Shared.Results;

namespace TP07B.Contracts;

public interface IAlertSubscriptionsService
{
    Task<OperationResult<PreferenceDto>> UpsertAsync(
        PreferenceDto preference,
        CancellationToken cancellationToken = default);

    Task<OperationResult<PreferenceDto>> GetAsync(
        string userId,
        string channel,
        CancellationToken cancellationToken = default);
}
