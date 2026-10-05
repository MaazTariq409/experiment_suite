using FluentAssertions;
using TP06A.Contracts;
using TP06A.Participant.Services;
using Xunit;

namespace TP06A.Evaluation;

/// <summary>HIDDEN EVALUATION — 20 tests for TP06-A Employee Import.</summary>
public class EmployeeImportServiceTests
{
    private static IEmployeeImportService Sut() => new EmployeeImportService();

    [Fact]
    public async Task NullRows_ReturnsValidationError()
    {
        var result = await Sut().ImportAsync(null!);
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public async Task EmptyRows_ReturnsZeroSummary()
    {
        var result = await Sut().ImportAsync([]);
        result.Succeeded.Should().BeTrue();
        result.Value!.Accepted.Should().Be(0);
        result.Value.Rejected.Should().Be(0);
        result.Value.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task AcceptsValidCsvRows()
    {
        var rows = new[] { new ImportRow(1, "K1,Name,10.5"), new ImportRow(2, "K2,Name2,3") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Accepted.Should().Be(2);
        result.Value.Rejected.Should().Be(0);
        result.Value.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task TrimsFields()
    {
        var rows = new[] { new ImportRow(1, " K1 , Alice , 5 ") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Accepted.Should().Be(1);
    }

    [Fact]
    public async Task RejectsMalformedRows_ButContinues()
    {
        var rows = new[] { new ImportRow(1, "K1,Name,10.5"), new ImportRow(2, "BAD") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Accepted.Should().Be(1);
        result.Value.Rejected.Should().Be(1);
        result.Value.Errors.Should().Contain(EmployeeImportRules.FormatError(2, EmployeeImportRules.Malformed));
    }

    [Fact]
    public async Task ExtraColumns_AreMalformed()
    {
        var rows = new[] { new ImportRow(1, "K1,Name,10,extra") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Rejected.Should().Be(1);
        result.Value.Errors.Single().Should().Be(EmployeeImportRules.FormatError(1, EmployeeImportRules.Malformed));
    }

    [Fact]
    public async Task BlankRaw_IsMalformed()
    {
        var rows = new[] { new ImportRow(1, "   ") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Rejected.Should().Be(1);
        result.Value.Errors.Single().Should().Contain(EmployeeImportRules.Malformed);
    }

    [Fact]
    public async Task EmptyKey_IsRejected()
    {
        var rows = new[] { new ImportRow(1, ",Name,10") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Errors.Single().Should().Be(EmployeeImportRules.FormatError(1, EmployeeImportRules.EmptyKey));
    }

    [Fact]
    public async Task EmptyName_IsRejected()
    {
        var rows = new[] { new ImportRow(1, "K1,,10") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Errors.Single().Should().Be(EmployeeImportRules.FormatError(1, EmployeeImportRules.EmptyName));
    }

    [Fact]
    public async Task NonNumericValue_IsRejected()
    {
        var rows = new[] { new ImportRow(1, "K1,Name,abc") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Errors.Single().Should().Be(EmployeeImportRules.FormatError(1, EmployeeImportRules.InvalidValue));
    }

    [Fact]
    public async Task NegativeValue_IsRejected()
    {
        var rows = new[] { new ImportRow(1, "K1,Name,-1") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Errors.Single().Should().Be(EmployeeImportRules.FormatError(1, EmployeeImportRules.InvalidValue));
    }

    [Fact]
    public async Task ZeroValue_IsAccepted()
    {
        var rows = new[] { new ImportRow(1, "K1,Name,0") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Accepted.Should().Be(1);
    }

    [Fact]
    public async Task DuplicateKey_IsRejected()
    {
        var rows = new[] { new ImportRow(1, "K1,Name,10.5"), new ImportRow(2, "K1,Name2,3") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Accepted.Should().Be(1);
        result.Value.Rejected.Should().Be(1);
        result.Value.Errors.Single().Should().Be(EmployeeImportRules.FormatError(2, EmployeeImportRules.DuplicateKey));
    }

    [Fact]
    public async Task DuplicateKey_IsCaseInsensitive()
    {
        var rows = new[] { new ImportRow(1, " Emp1 ,Ada,1"), new ImportRow(2, "emp1,Grace,2") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Accepted.Should().Be(1);
        result.Value.Rejected.Should().Be(1);
        result.Value.Errors.Single().Should().Contain(EmployeeImportRules.DuplicateKey);
    }

    [Fact]
    public async Task InvalidLineNumber_IsMalformed()
    {
        var rows = new[] { new ImportRow(0, "K1,Name,1") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Rejected.Should().Be(1);
        result.Value.Errors.Single().Should().Contain(EmployeeImportRules.Malformed);
    }

    [Fact]
    public async Task MixedRows_CountsMatch()
    {
        var rows = new[]
        {
            new ImportRow(1, "K1,Name,1"),
            new ImportRow(2, "BAD"),
            new ImportRow(3, "K2,Name,2"),
            new ImportRow(4, "K1,Dup,3")
        };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Accepted.Should().Be(2);
        result.Value.Rejected.Should().Be(2);
        result.Value.Errors.Should().HaveCount(2);
        (result.Value.Accepted + result.Value.Rejected).Should().Be(4);
    }

    [Fact]
    public async Task Errors_AreOrderedByLineNumber()
    {
        var rows = new[]
        {
            new ImportRow(3, "BAD"),
            new ImportRow(1, "ALSOBAD"),
            new ImportRow(2, "K1,Name,1")
        };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Errors.Should().HaveCount(2);
        result.Value.Errors[0].Should().StartWith("Line 1:");
        result.Value.Errors[1].Should().StartWith("Line 3:");
    }

    [Fact]
    public async Task InvariantCulture_DecimalAccepted()
    {
        var rows = new[] { new ImportRow(1, "K1,Name,10.25") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Accepted.Should().Be(1);
    }

    [Fact]
    public async Task CommaOnlySeparators_WithMissingParts_RejectCorrectly()
    {
        var rows = new[] { new ImportRow(1, "K1,") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Rejected.Should().Be(1);
        result.Value.Errors.Single().Should().Contain(EmployeeImportRules.Malformed);
    }

    [Fact]
    public async Task LaterValidRow_StillAcceptedAfterErrors()
    {
        var rows = new[]
        {
            new ImportRow(1, "BAD"),
            new ImportRow(2, "K9,Final,99")
        };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Accepted.Should().Be(1);
        result.Value.Rejected.Should().Be(1);
    }
}
