using Enterprise.Shared.Time;
using FluentAssertions;
using Xunit;
using TP01B.Contracts;
using TP01B.Participant.Persistence;
using TP01B.Participant.Services;

namespace TP01B.Evaluation;

public class SupplierContractServiceTests
{
    private static ISupplierContractService CreateSut(DateTimeOffset? now = null)
    {
        var clock = new FixedClock(now ?? new DateTimeOffset(2026, 3, 15, 12, 0, 0, TimeSpan.Zero));
        return new SupplierContractService(new InMemorySupplierContractRepository(), clock);
    }

    private static SupplierContractCreateRequest ValidCreate(string code = "EQ-1001") => new()
    {
        Code = code,
        Title = "Quarterly inspection",
        ScheduledDate = new DateOnly(2026, 4, 1),
        EstimatedCost = 250.50m,
        Status = "Draft"
    };

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreatedEntity()
    {
        var sut = CreateSut();
        var result = await sut.CreateAsync(ValidCreate());
        result.Succeeded.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Value.Should().NotBeNull();
        result.Value!.Code.Should().Be("EQ-1001");
        result.Value.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Create_MissingTitle_ReturnsValidationError()
    {
        var sut = CreateSut();
        var request = ValidCreate();
        request.Title = " ";
        var result = await sut.CreateAsync(request);
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public async Task Create_NegativeCost_ReturnsValidationError()
    {
        var sut = CreateSut();
        var request = ValidCreate();
        request.EstimatedCost = -1m;
        var result = await sut.CreateAsync(request);
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Create_DuplicateCode_ReturnsConflict()
    {
        var sut = CreateSut();
        (await sut.CreateAsync(ValidCreate())).Succeeded.Should().BeTrue();
        var result = await sut.CreateAsync(ValidCreate());
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.ErrorCode.Should().Be("DUPLICATE_CODE");
    }

    [Fact]
    public async Task GetById_Missing_ReturnsNotFound()
    {
        var sut = CreateSut();
        var result = await sut.GetByIdAsync(Guid.NewGuid());
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetById_Existing_ReturnsEntity()
    {
        var sut = CreateSut();
        var created = await sut.CreateAsync(ValidCreate());
        var result = await sut.GetByIdAsync(created.Value!.Id);
        result.Succeeded.Should().BeTrue();
        result.Value!.Title.Should().Be("Quarterly inspection");
    }

    [Fact]
    public async Task Update_Existing_UpdatesMutableFields()
    {
        var sut = CreateSut();
        var created = await sut.CreateAsync(ValidCreate());
        var result = await sut.UpdateAsync(created.Value!.Id, new SupplierContractUpdateRequest
        {
            Title = "Updated title",
            ScheduledDate = new DateOnly(2026, 5, 1),
            EstimatedCost = 300m,
            Status = "Scheduled"
        });
        result.Succeeded.Should().BeTrue();
        result.Value!.Title.Should().Be("Updated title");
        result.Value.Status.Should().Be("Scheduled");
        result.Value.Code.Should().Be("EQ-1001");
    }

    [Fact]
    public async Task Update_Missing_ReturnsNotFound()
    {
        var sut = CreateSut();
        var result = await sut.UpdateAsync(Guid.NewGuid(), new SupplierContractUpdateRequest
        {
            Title = "X",
            ScheduledDate = new DateOnly(2026, 5, 1),
            EstimatedCost = 10m,
            Status = "Draft"
        });
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Update_ClosedStatus_RejectsMutation()
    {
        var sut = CreateSut();
        var created = await sut.CreateAsync(ValidCreate());
        (await sut.UpdateAsync(created.Value!.Id, new SupplierContractUpdateRequest
        {
            Title = "Close me",
            ScheduledDate = new DateOnly(2026, 4, 1),
            EstimatedCost = 250.50m,
            Status = "Closed"
        })).Succeeded.Should().BeTrue();

        var result = await sut.UpdateAsync(created.Value.Id, new SupplierContractUpdateRequest
        {
            Title = "Should fail",
            ScheduledDate = new DateOnly(2026, 6, 1),
            EstimatedCost = 1m,
            Status = "Draft"
        });
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        result.ErrorCode.Should().Be("INVALID_STATE");
    }

    [Fact]
    public async Task Create_InvalidStatus_ReturnsValidationError()
    {
        var sut = CreateSut();
        var request = ValidCreate();
        request.Status = "Unknown";
        var result = await sut.CreateAsync(request);
        result.Succeeded.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }
}
