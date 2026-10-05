using FluentAssertions;
using TP06B.Contracts;
using TP06B.Participant.Services;
using Xunit;

namespace TP06B.Evaluation;

/// <summary>HIDDEN EVALUATION — 20 tests for TP06-B Product Catalog Import.</summary>
public class ProductCatalogImportServiceTests
{
    private static IProductCatalogImportService Sut() => new ProductCatalogImportService();

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
        var rows = new[] { new ImportRow(1, "P1,Widget,10.5"), new ImportRow(2, "P2,Gadget,3") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Accepted.Should().Be(2);
        result.Value.Rejected.Should().Be(0);
        result.Value.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task TrimsFields()
    {
        var rows = new[] { new ImportRow(1, " P1 , Widget , 5 ") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Accepted.Should().Be(1);
    }

    [Fact]
    public async Task RejectsMalformedRows_ButContinues()
    {
        var rows = new[] { new ImportRow(1, "P1,Widget,10.5"), new ImportRow(2, "BAD") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Accepted.Should().Be(1);
        result.Value.Rejected.Should().Be(1);
        result.Value.Errors.Should().Contain(ProductImportRules.FormatError(2, ProductImportRules.Malformed));
    }

    [Fact]
    public async Task ExtraColumns_AreMalformed()
    {
        var rows = new[] { new ImportRow(1, "P1,Widget,10,extra") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Rejected.Should().Be(1);
        result.Value.Errors.Single().Should().Be(ProductImportRules.FormatError(1, ProductImportRules.Malformed));
    }

    [Fact]
    public async Task BlankRaw_IsMalformed()
    {
        var rows = new[] { new ImportRow(1, "   ") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Rejected.Should().Be(1);
        result.Value.Errors.Single().Should().Contain(ProductImportRules.Malformed);
    }

    [Fact]
    public async Task EmptyKey_IsRejected()
    {
        var rows = new[] { new ImportRow(1, ",Widget,10") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Errors.Single().Should().Be(ProductImportRules.FormatError(1, ProductImportRules.EmptyKey));
    }

    [Fact]
    public async Task EmptyName_IsRejected()
    {
        var rows = new[] { new ImportRow(1, "P1,,10") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Errors.Single().Should().Be(ProductImportRules.FormatError(1, ProductImportRules.EmptyName));
    }

    [Fact]
    public async Task NonNumericValue_IsRejected()
    {
        var rows = new[] { new ImportRow(1, "P1,Widget,abc") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Errors.Single().Should().Be(ProductImportRules.FormatError(1, ProductImportRules.InvalidValue));
    }

    [Fact]
    public async Task NegativeValue_IsRejected()
    {
        var rows = new[] { new ImportRow(1, "P1,Widget,-1") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Errors.Single().Should().Be(ProductImportRules.FormatError(1, ProductImportRules.InvalidValue));
    }

    [Fact]
    public async Task ZeroValue_IsAccepted()
    {
        var rows = new[] { new ImportRow(1, "P1,Widget,0") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Accepted.Should().Be(1);
    }

    [Fact]
    public async Task DuplicateKey_IsRejected()
    {
        var rows = new[] { new ImportRow(1, "P1,Widget,10.5"), new ImportRow(2, "P1,Gadget,3") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Accepted.Should().Be(1);
        result.Value.Rejected.Should().Be(1);
        result.Value.Errors.Single().Should().Be(ProductImportRules.FormatError(2, ProductImportRules.DuplicateKey));
    }

    [Fact]
    public async Task DuplicateKey_IsCaseInsensitive()
    {
        var rows = new[] { new ImportRow(1, " Sku1 ,Ada,1"), new ImportRow(2, "sku1,Grace,2") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Accepted.Should().Be(1);
        result.Value.Rejected.Should().Be(1);
        result.Value.Errors.Single().Should().Contain(ProductImportRules.DuplicateKey);
    }

    [Fact]
    public async Task InvalidLineNumber_IsMalformed()
    {
        var rows = new[] { new ImportRow(0, "P1,Widget,1") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Rejected.Should().Be(1);
        result.Value.Errors.Single().Should().Contain(ProductImportRules.Malformed);
    }

    [Fact]
    public async Task MixedRows_CountsMatch()
    {
        var rows = new[]
        {
            new ImportRow(1, "P1,Widget,1"),
            new ImportRow(2, "BAD"),
            new ImportRow(3, "P2,Gadget,2"),
            new ImportRow(4, "P1,Dup,3")
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
            new ImportRow(2, "P1,Widget,1")
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
        var rows = new[] { new ImportRow(1, "P1,Widget,10.25") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Accepted.Should().Be(1);
    }

    [Fact]
    public async Task CommaOnlySeparators_WithMissingParts_RejectCorrectly()
    {
        var rows = new[] { new ImportRow(1, "P1,") };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Rejected.Should().Be(1);
        result.Value.Errors.Single().Should().Contain(ProductImportRules.Malformed);
    }

    [Fact]
    public async Task LaterValidRow_StillAcceptedAfterErrors()
    {
        var rows = new[]
        {
            new ImportRow(1, "BAD"),
            new ImportRow(2, "P9,Final,99")
        };
        var result = await Sut().ImportAsync(rows);
        result.Succeeded.Should().BeTrue();
        result.Value!.Accepted.Should().Be(1);
        result.Value.Rejected.Should().Be(1);
    }
}
