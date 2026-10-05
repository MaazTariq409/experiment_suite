namespace TP07A.Contracts;

public sealed record PreferenceDto(string UserId, string Channel, bool Enabled, string Frequency);

/// <summary>Frozen vocabulary for Notification Preferences (TP07-A).</summary>
public static class NotificationPreferenceRules
{
    public static readonly HashSet<string> Channels = new(StringComparer.OrdinalIgnoreCase)
    {
        "Email", "Sms", "Push"
    };

    public static readonly HashSet<string> Frequencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "Immediate", "Daily", "Weekly"
    };

    public static string CanonicalChannel(string channel) => channel.Trim().ToLowerInvariant() switch
    {
        "email" => "Email",
        "sms" => "Sms",
        "push" => "Push",
        _ => channel.Trim()
    };

    public static string CanonicalFrequency(string frequency) => frequency.Trim().ToLowerInvariant() switch
    {
        "immediate" => "Immediate",
        "daily" => "Daily",
        "weekly" => "Weekly",
        _ => frequency.Trim()
    };
}
