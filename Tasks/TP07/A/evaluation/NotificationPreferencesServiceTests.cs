using FluentAssertions;
using TP07A.Contracts;
using TP07A.Participant.Services;
using Xunit;

namespace TP07A.Evaluation;

/// <summary>HIDDEN EVALUATION — 20 tests for TP07-A Notification Preferences.</summary>
public class NotificationPreferencesServiceTests
{
    private static INotificationPreferencesService Sut() => new NotificationPreferencesService();

    private static PreferenceDto Valid(
        string userId = "u1",
        string channel = "Email",
        bool enabled = true,
        string frequency = "Daily") =>
        new(userId, channel, enabled, frequency);

    [Fact]
    public async Task Upsert_Null_ReturnsValidationError()
    {
        var result = await Sut().UpsertAsync(null!);
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public async Task Upsert_RejectsBlankUserId()
    {
        var result = await Sut().UpsertAsync(Valid(userId: " "));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public async Task Upsert_RejectsBlankChannel()
    {
        var result = await Sut().UpsertAsync(Valid(channel: ""));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Upsert_RejectsUnknownChannel()
    {
        var result = await Sut().UpsertAsync(Valid(channel: "Fax"));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Upsert_RejectsUnknownFrequency()
    {
        var result = await Sut().UpsertAsync(Valid(frequency: "Yearly"));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Upsert_NormalizesCanonicalValues()
    {
        var result = await Sut().UpsertAsync(Valid(userId: " u1 ", channel: "email", frequency: "daily"));
        result.Succeeded.Should().BeTrue();
        result.Value!.UserId.Should().Be("u1");
        result.Value.Channel.Should().Be("Email");
        result.Value.Frequency.Should().Be("Daily");
    }

    [Fact]
    public async Task Upsert_ThenGet_RoundTrips()
    {
        var sut = Sut();
        await sut.UpsertAsync(Valid(frequency: "Weekly"));
        var result = await sut.GetAsync("u1", "Email");
        result.Succeeded.Should().BeTrue();
        result.Value!.Frequency.Should().Be("Weekly");
        result.Value.Enabled.Should().BeTrue();
    }

    [Fact]
    public async Task Get_Missing_Returns404()
    {
        var result = await Sut().GetAsync("u1", "Email");
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.ErrorCode.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task Get_BlankUserId_ReturnsValidationError()
    {
        var result = await Sut().GetAsync(" ", "Email");
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Get_UnknownChannel_ReturnsValidationError()
    {
        var result = await Sut().GetAsync("u1", "Fax");
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Lookup_IsCaseInsensitive()
    {
        var sut = Sut();
        await sut.UpsertAsync(Valid(userId: "UserA", channel: "SMS", frequency: "Immediate"));
        var result = await sut.GetAsync("usera", "sms");
        result.Succeeded.Should().BeTrue();
        result.Value!.Channel.Should().Be("Sms");
        result.Value.Frequency.Should().Be("Immediate");
    }

    [Fact]
    public async Task Upsert_OverwritesExisting()
    {
        var sut = Sut();
        await sut.UpsertAsync(Valid(enabled: true, frequency: "Daily"));
        var updated = await sut.UpsertAsync(Valid(enabled: false, frequency: "Weekly"));
        updated.Succeeded.Should().BeTrue();
        updated.Value!.Enabled.Should().BeFalse();
        var got = await sut.GetAsync("u1", "Email");
        got.Value!.Frequency.Should().Be("Weekly");
        got.Value.Enabled.Should().BeFalse();
    }

    [Fact]
    public async Task DifferentChannels_AreIndependent()
    {
        var sut = Sut();
        await sut.UpsertAsync(Valid(channel: "Email", frequency: "Daily"));
        await sut.UpsertAsync(Valid(channel: "Push", frequency: "Immediate"));
        var email = await sut.GetAsync("u1", "Email");
        var push = await sut.GetAsync("u1", "Push");
        email.Value!.Frequency.Should().Be("Daily");
        push.Value!.Frequency.Should().Be("Immediate");
    }

    [Fact]
    public async Task DifferentUsers_AreIndependent()
    {
        var sut = Sut();
        await sut.UpsertAsync(Valid(userId: "u1", frequency: "Daily"));
        await sut.UpsertAsync(Valid(userId: "u2", frequency: "Weekly"));
        (await sut.GetAsync("u1", "Email")).Value!.Frequency.Should().Be("Daily");
        (await sut.GetAsync("u2", "Email")).Value!.Frequency.Should().Be("Weekly");
    }

    [Fact]
    public async Task AcceptsAllChannels()
    {
        var sut = Sut();
        foreach (var channel in new[] { "Email", "Sms", "Push" })
        {
            var result = await sut.UpsertAsync(Valid(channel: channel));
            result.Succeeded.Should().BeTrue();
        }
    }

    [Fact]
    public async Task AcceptsAllFrequencies()
    {
        var sut = Sut();
        foreach (var frequency in new[] { "Immediate", "Daily", "Weekly" })
        {
            var result = await sut.UpsertAsync(Valid(userId: "u-" + frequency, frequency: frequency));
            result.Succeeded.Should().BeTrue();
        }
    }

    [Fact]
    public async Task DisabledPreference_RoundTrips()
    {
        var sut = Sut();
        await sut.UpsertAsync(Valid(enabled: false));
        var result = await sut.GetAsync("u1", "Email");
        result.Succeeded.Should().BeTrue();
        result.Value!.Enabled.Should().BeFalse();
    }

    [Fact]
    public async Task Get_DoesNotReturnWrongChannel()
    {
        var sut = Sut();
        await sut.UpsertAsync(Valid(channel: "Email"));
        var result = await sut.GetAsync("u1", "Push");
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Upsert_BlankFrequency_ReturnsValidationError()
    {
        var result = await Sut().UpsertAsync(Valid(frequency: " "));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Get_TrimsInputs()
    {
        var sut = Sut();
        await sut.UpsertAsync(Valid(userId: "u9", channel: "Email"));
        var result = await sut.GetAsync(" u9 ", " Email ");
        result.Succeeded.Should().BeTrue();
        result.Value!.UserId.Should().Be("u9");
    }
}
