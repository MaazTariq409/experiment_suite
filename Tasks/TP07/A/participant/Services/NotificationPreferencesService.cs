using Enterprise.Shared.Results;
using TP07A.Contracts;

namespace TP07A.Participant.Services;

/// <summary>
/// LEGACY STARTER with intentional defects and code smells.
/// Fix behavioral defects and improve structure; keep the frozen interface.
/// Known defects (must fix):
/// 1) Upsert accepts blank UserId/Channel and invalid Frequency/Channel
/// 2) Get returns 400/BAD_REQUEST for missing records instead of 404/NOT_FOUND
/// 3) Lookup keys are case-sensitive
/// 4) Duplicated key construction / magic separators (smell to clean up)
/// </summary>
public sealed class NotificationPreferencesService : INotificationPreferencesService
{
    private readonly Dictionary<string, PreferenceDto> _data = new();

    public Task<OperationResult<PreferenceDto>> UpsertAsync(
        PreferenceDto preference,
        CancellationToken cancellationToken = default)
    {
        // Defect: accepts blank UserId; Smell: duplicated key logic / magic strings
        var key = preference.UserId + ":" + preference.Channel;
        _data[key] = preference;
        return Task.FromResult(OperationResult<PreferenceDto>.Ok(preference));
    }

    public Task<OperationResult<PreferenceDto>> GetAsync(
        string userId,
        string channel,
        CancellationToken cancellationToken = default)
    {
        var key = userId + ":" + channel;
        if (_data.TryGetValue(key, out var value))
            return Task.FromResult(OperationResult<PreferenceDto>.Ok(value));
        // Defect: returns 400 instead of 404
        return Task.FromResult(OperationResult<PreferenceDto>.Fail("BAD_REQUEST", "missing", 400));
    }
}
