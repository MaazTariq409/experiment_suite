using Enterprise.Shared.Results;
using TP07A.Contracts;

namespace TP07A.Participant.Services;

/// <summary>REFERENCE IMPLEMENTATION — corrected behavior for evaluator verification.</summary>
public sealed class NotificationPreferencesService : INotificationPreferencesService
{
    private readonly Dictionary<string, PreferenceDto> _store =
        new(StringComparer.OrdinalIgnoreCase);

    public Task<OperationResult<PreferenceDto>> UpsertAsync(
        PreferenceDto preference,
        CancellationToken cancellationToken = default)
    {
        if (preference is null)
            return Fail("Request is required.");

        var userId = preference.UserId?.Trim() ?? string.Empty;
        var channelRaw = preference.Channel?.Trim() ?? string.Empty;
        var frequencyRaw = preference.Frequency?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(userId) ||
            !NotificationPreferenceRules.Channels.Contains(channelRaw) ||
            !NotificationPreferenceRules.Frequencies.Contains(frequencyRaw))
        {
            return Fail("Invalid preference.");
        }

        var normalized = new PreferenceDto(
            userId,
            NotificationPreferenceRules.CanonicalChannel(channelRaw),
            preference.Enabled,
            NotificationPreferenceRules.CanonicalFrequency(frequencyRaw));

        _store[BuildKey(normalized.UserId, normalized.Channel)] = normalized;
        return Task.FromResult(OperationResult<PreferenceDto>.Ok(normalized));
    }

    public Task<OperationResult<PreferenceDto>> GetAsync(
        string userId,
        string channel,
        CancellationToken cancellationToken = default)
    {
        var uid = userId?.Trim() ?? string.Empty;
        var ch = channel?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(uid) || !NotificationPreferenceRules.Channels.Contains(ch))
            return Fail("Invalid lookup.");

        if (_store.TryGetValue(BuildKey(uid, NotificationPreferenceRules.CanonicalChannel(ch)), out var value))
            return Task.FromResult(OperationResult<PreferenceDto>.Ok(value));

        return Task.FromResult(OperationResult<PreferenceDto>.Fail("NOT_FOUND", "Preference not found.", 404));
    }

    private static string BuildKey(string userId, string channel) => $"{userId}|{channel}";

    private static Task<OperationResult<PreferenceDto>> Fail(string message) =>
        Task.FromResult(OperationResult<PreferenceDto>.Fail("VALIDATION_ERROR", message, 400));
}
