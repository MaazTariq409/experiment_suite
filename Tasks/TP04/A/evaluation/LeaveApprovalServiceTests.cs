using FluentAssertions;
using TP04A.Contracts;
using TP04A.Participant.Services;
using Xunit;

namespace TP04A.Evaluation;

/// <summary>HIDDEN EVALUATION — 20 tests for TP04-A Leave Approval.</summary>
public class LeaveApprovalServiceTests
{
    private static ILeaveApprovalService Sut() => new LeaveApprovalService();

    private static TransitionRequest Req(
        string from,
        string to,
        string role,
        string? comment = "ok",
        Guid? id = null) =>
        new(id ?? Guid.NewGuid(), from, to, role, comment);

    [Fact]
    public async Task EmptyId_ReturnsValidationError()
    {
        var result = await Sut().TransitionAsync(Req("Draft", "Submitted", "Employee", id: Guid.Empty));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public async Task UnknownStatus_ReturnsValidationError()
    {
        var result = await Sut().TransitionAsync(Req("Draft", "Flying", "Employee"));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task UnknownRole_ReturnsValidationError()
    {
        var result = await Sut().TransitionAsync(Req("Draft", "Submitted", "Intern"));
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Draft_To_Submitted_ByEmployee_Succeeds()
    {
        var result = await Sut().TransitionAsync(Req("Draft", "Submitted", "Employee", comment: null));
        result.Succeeded.Should().BeTrue();
        result.Value!.Applied.Should().BeTrue();
        result.Value.NewStatus.Should().Be("Submitted");
        result.Value.ReasonCode.Should().Be("APPLIED");
    }

    [Fact]
    public async Task Submitted_To_Approved_ByManager_Succeeds()
    {
        var result = await Sut().TransitionAsync(Req("Submitted", "Approved", "Manager", "ok"));
        result.Succeeded.Should().BeTrue();
        result.Value!.Applied.Should().BeTrue();
        result.Value.NewStatus.Should().Be("Approved");
    }

    [Fact]
    public async Task Submitted_To_Approved_ByAdmin_Succeeds()
    {
        var result = await Sut().TransitionAsync(Req("Submitted", "Approved", "Admin", "approve"));
        result.Succeeded.Should().BeTrue();
        result.Value!.Applied.Should().BeTrue();
    }

    [Fact]
    public async Task Submitted_To_Rejected_ByManager_Succeeds()
    {
        var result = await Sut().TransitionAsync(Req("Submitted", "Rejected", "Manager", "missing docs"));
        result.Succeeded.Should().BeTrue();
        result.Value!.Applied.Should().BeTrue();
        result.Value.NewStatus.Should().Be("Rejected");
    }

    [Fact]
    public async Task Submitted_To_Cancelled_ByEmployee_Succeeds()
    {
        var result = await Sut().TransitionAsync(Req("Submitted", "Cancelled", "Employee", null));
        result.Succeeded.Should().BeTrue();
        result.Value!.Applied.Should().BeTrue();
        result.Value.NewStatus.Should().Be("Cancelled");
    }

    [Fact]
    public async Task Draft_To_Cancelled_ByEmployee_Succeeds()
    {
        var result = await Sut().TransitionAsync(Req("Draft", "Cancelled", "Employee", null));
        result.Succeeded.Should().BeTrue();
        result.Value!.Applied.Should().BeTrue();
    }

    [Fact]
    public async Task Draft_To_Approved_IsInvalidTransition()
    {
        var result = await Sut().TransitionAsync(Req("Draft", "Approved", "Manager", "ok"));
        result.Succeeded.Should().BeTrue();
        result.Value!.Applied.Should().BeFalse();
        result.Value.ReasonCode.Should().Be("INVALID_TRANSITION");
        result.Value.NewStatus.Should().Be("Draft");
    }

    [Fact]
    public async Task Approved_IsTerminal()
    {
        var result = await Sut().TransitionAsync(Req("Approved", "Cancelled", "Admin", "no"));
        result.Succeeded.Should().BeTrue();
        result.Value!.Applied.Should().BeFalse();
        result.Value.ReasonCode.Should().Be("INVALID_TRANSITION");
        result.Value.NewStatus.Should().Be("Approved");
    }

    [Fact]
    public async Task Rejected_IsTerminal()
    {
        var result = await Sut().TransitionAsync(Req("Rejected", "Submitted", "Manager", "retry"));
        result.Succeeded.Should().BeTrue();
        result.Value!.Applied.Should().BeFalse();
        result.Value.ReasonCode.Should().Be("INVALID_TRANSITION");
    }

    [Fact]
    public async Task Cancelled_IsTerminal()
    {
        var result = await Sut().TransitionAsync(Req("Cancelled", "Draft", "Employee", null));
        result.Succeeded.Should().BeTrue();
        result.Value!.Applied.Should().BeFalse();
        result.Value.ReasonCode.Should().Be("INVALID_TRANSITION");
    }

    [Fact]
    public async Task Employee_CannotApprove()
    {
        var result = await Sut().TransitionAsync(Req("Submitted", "Approved", "Employee", "no"));
        result.Succeeded.Should().BeTrue();
        result.Value!.Applied.Should().BeFalse();
        result.Value.ReasonCode.Should().Be("FORBIDDEN");
        result.Value.NewStatus.Should().Be("Submitted");
    }

    [Fact]
    public async Task Employee_CannotReject()
    {
        var result = await Sut().TransitionAsync(Req("Submitted", "Rejected", "Employee", "no"));
        result.Succeeded.Should().BeTrue();
        result.Value!.Applied.Should().BeFalse();
        result.Value.ReasonCode.Should().Be("FORBIDDEN");
    }

    [Fact]
    public async Task Approve_WithoutComment_RequiresComment()
    {
        var result = await Sut().TransitionAsync(Req("Submitted", "Approved", "Manager", "  "));
        result.Succeeded.Should().BeTrue();
        result.Value!.Applied.Should().BeFalse();
        result.Value.ReasonCode.Should().Be("COMMENT_REQUIRED");
        result.Value.NewStatus.Should().Be("Submitted");
    }

    [Fact]
    public async Task Reject_WithoutComment_RequiresComment()
    {
        var result = await Sut().TransitionAsync(Req("Submitted", "Rejected", "Admin", null));
        result.Succeeded.Should().BeTrue();
        result.Value!.Applied.Should().BeFalse();
        result.Value.ReasonCode.Should().Be("COMMENT_REQUIRED");
    }

    [Fact]
    public async Task CaseInsensitive_StatusesAndRoles()
    {
        var result = await Sut().TransitionAsync(Req("submitted", "approved", "manager", "ok"));
        result.Succeeded.Should().BeTrue();
        result.Value!.Applied.Should().BeTrue();
        result.Value.NewStatus.Should().Be("Approved");
    }

    [Fact]
    public async Task Manager_CannotJump_SubmittedToDraft()
    {
        var result = await Sut().TransitionAsync(Req("Submitted", "Draft", "Manager", "back"));
        result.Succeeded.Should().BeTrue();
        result.Value!.Applied.Should().BeFalse();
        result.Value.ReasonCode.Should().Be("INVALID_TRANSITION");
    }

    [Fact]
    public async Task Admin_CanSubmit_FromDraft()
    {
        var result = await Sut().TransitionAsync(Req("Draft", "Submitted", "Admin", null));
        result.Succeeded.Should().BeTrue();
        result.Value!.Applied.Should().BeTrue();
        result.Value.NewStatus.Should().Be("Submitted");
    }
}
