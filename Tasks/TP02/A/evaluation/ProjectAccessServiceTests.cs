using Enterprise.Shared.Results;
using FluentAssertions;
using TP02A.Contracts;
using TP02A.Participant.Services;
using Xunit;

namespace TP02A.Evaluation;

/// <summary>
/// HIDDEN EVALUATION — 20 tests mapped to TP02-A requirements.
/// Not distributed in participant packages.
/// </summary>
public class ProjectAccessServiceTests
{
    private static IProjectAccessService Sut() => new ProjectAccessService();

    private static AccessRequest Req(
        string role = "Admin",
        string action = "View",
        string status = "Open",
        string userId = "u1",
        string resourceId = "PRJ-100") =>
        new(userId, role, resourceId, action, status);

    // --- Validation ---

    [Fact]
    public async Task BlankUserId_ReturnsValidationError()
    {
        var result = await Sut().AuthorizeAsync(Req(userId: " "));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public async Task BlankResourceId_ReturnsValidationError()
    {
        var result = await Sut().AuthorizeAsync(Req(resourceId: ""));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public async Task UnknownRole_ReturnsValidationError()
    {
        var result = await Sut().AuthorizeAsync(Req(role: "Intern"));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public async Task UnknownAction_ReturnsValidationError()
    {
        var result = await Sut().AuthorizeAsync(Req(action: "Explode"));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public async Task UnknownStatus_ReturnsValidationError()
    {
        var result = await Sut().AuthorizeAsync(Req(status: "Deleted"));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }

    // --- Closed-state business rule ---

    [Fact]
    public async Task Closed_RejectsUpdate()
    {
        var result = await Sut().AuthorizeAsync(Req(role: "Admin", action: "Update", status: "Closed"));
        result.Succeeded.Should().BeTrue();
        result.Value!.Allowed.Should().BeFalse();
        result.Value.ReasonCode.Should().Be("INVALID_STATE");
    }

    [Fact]
    public async Task Closed_RejectsApprove()
    {
        var result = await Sut().AuthorizeAsync(Req(role: "Manager", action: "Approve", status: "Closed"));
        result.Succeeded.Should().BeTrue();
        result.Value!.Allowed.Should().BeFalse();
        result.Value.ReasonCode.Should().Be("INVALID_STATE");
    }

    [Fact]
    public async Task Closed_RejectsArchive()
    {
        var result = await Sut().AuthorizeAsync(Req(role: "Admin", action: "Archive", status: "Closed"));
        result.Succeeded.Should().BeTrue();
        result.Value!.Allowed.Should().BeFalse();
        result.Value.ReasonCode.Should().Be("INVALID_STATE");
    }

    [Fact]
    public async Task Closed_AllowsView_ForViewer()
    {
        var result = await Sut().AuthorizeAsync(Req(role: "Viewer", action: "View", status: "Closed"));
        result.Succeeded.Should().BeTrue();
        result.Value!.Allowed.Should().BeTrue();
        result.Value.ReasonCode.Should().Be("ALLOWED");
    }

    // --- Role matrix ---

    [Fact]
    public async Task Admin_CanApprove_Open()
    {
        var result = await Sut().AuthorizeAsync(Req(role: "Admin", action: "Approve", status: "Open"));
        result.Succeeded.Should().BeTrue();
        result.Value!.Allowed.Should().BeTrue();
        result.Value.ReasonCode.Should().Be("ALLOWED");
    }

    [Fact]
    public async Task Admin_CanArchive_InReview()
    {
        var result = await Sut().AuthorizeAsync(Req(role: "Admin", action: "Archive", status: "InReview"));
        result.Succeeded.Should().BeTrue();
        result.Value!.Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task Manager_CanApprove_InReview()
    {
        var result = await Sut().AuthorizeAsync(Req(role: "Manager", action: "Approve", status: "InReview"));
        result.Succeeded.Should().BeTrue();
        result.Value!.Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task Manager_CannotArchive()
    {
        var result = await Sut().AuthorizeAsync(Req(role: "Manager", action: "Archive", status: "Open"));
        result.Succeeded.Should().BeTrue();
        result.Value!.Allowed.Should().BeFalse();
        result.Value.ReasonCode.Should().Be("FORBIDDEN");
    }

    [Fact]
    public async Task Contributor_CanUpdate_Open()
    {
        var result = await Sut().AuthorizeAsync(Req(role: "Contributor", action: "Update", status: "Open"));
        result.Succeeded.Should().BeTrue();
        result.Value!.Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task Contributor_CannotApprove()
    {
        var result = await Sut().AuthorizeAsync(Req(role: "Contributor", action: "Approve", status: "Open"));
        result.Succeeded.Should().BeTrue();
        result.Value!.Allowed.Should().BeFalse();
        result.Value.ReasonCode.Should().Be("FORBIDDEN");
    }

    [Fact]
    public async Task Contributor_CannotUpdate_InReview()
    {
        var result = await Sut().AuthorizeAsync(Req(role: "Contributor", action: "Update", status: "InReview"));
        result.Succeeded.Should().BeTrue();
        result.Value!.Allowed.Should().BeFalse();
        result.Value.ReasonCode.Should().Be("FORBIDDEN");
    }

    [Fact]
    public async Task Viewer_CanView_Open()
    {
        var result = await Sut().AuthorizeAsync(Req(role: "Viewer", action: "View", status: "Open"));
        result.Succeeded.Should().BeTrue();
        result.Value!.Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task Viewer_CannotUpdate()
    {
        var result = await Sut().AuthorizeAsync(Req(role: "Viewer", action: "Update", status: "Open"));
        result.Succeeded.Should().BeTrue();
        result.Value!.Allowed.Should().BeFalse();
        result.Value.ReasonCode.Should().Be("FORBIDDEN");
    }

    [Fact]
    public async Task Viewer_CannotApprove()
    {
        var result = await Sut().AuthorizeAsync(Req(role: "Viewer", action: "Approve", status: "Open"));
        result.Succeeded.Should().BeTrue();
        result.Value!.Allowed.Should().BeFalse();
        result.Value.ReasonCode.Should().Be("FORBIDDEN");
    }

    [Fact]
    public async Task RoleComparison_IsCaseInsensitive()
    {
        var result = await Sut().AuthorizeAsync(Req(role: "admin", action: "view", status: "open"));
        result.Succeeded.Should().BeTrue();
        result.Value!.Allowed.Should().BeTrue();
        result.Value.ReasonCode.Should().Be("ALLOWED");
    }
}
