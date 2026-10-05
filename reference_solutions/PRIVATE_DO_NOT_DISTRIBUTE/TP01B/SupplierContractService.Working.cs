using Enterprise.Shared.Results;
using Enterprise.Shared.Time;
using TP01B.Contracts;

namespace TP01B.Participant.Services;

/// <summary>
/// REFERENCE IMPLEMENTATION used to validate the hidden evaluator for Variant B.
/// </summary>
public sealed class SupplierContractService : ISupplierContractService
{
    private static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        "Draft", "Scheduled", "InProgress", "Closed"
    };

    private readonly ISupplierContractRepository _repository;
    private readonly IClock _clock;

    public SupplierContractService(ISupplierContractRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<OperationResult<SupplierContractResponse>> CreateAsync(
        SupplierContractCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var validation = Validate(request.Title, request.EstimatedCost, request.Status, request.Code);
        if (validation is not null)
            return validation;

        if (await _repository.CodeExistsAsync(request.Code, cancellationToken))
            return OperationResult<SupplierContractResponse>.Fail("DUPLICATE_CODE", "Code already exists.", 409);

        var entity = new SupplierContractResponse
        {
            Id = Guid.NewGuid(),
            Code = request.Code.Trim(),
            Title = request.Title.Trim(),
            ScheduledDate = request.ScheduledDate,
            EstimatedCost = request.EstimatedCost,
            Status = request.Status,
            CreatedAtUtc = _clock.UtcNow
        };

        await _repository.AddAsync(entity, cancellationToken);
        return OperationResult<SupplierContractResponse>.Ok(entity, 201);
    }

    public async Task<OperationResult<SupplierContractResponse>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByIdAsync(id, cancellationToken);
        return entity is null
            ? OperationResult<SupplierContractResponse>.Fail("NOT_FOUND", "Record not found.", 404)
            : OperationResult<SupplierContractResponse>.Ok(entity);
    }

    public async Task<OperationResult<SupplierContractResponse>> UpdateAsync(
        Guid id,
        SupplierContractUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return OperationResult<SupplierContractResponse>.Fail("NOT_FOUND", "Record not found.", 404);

        if (string.Equals(existing.Status, "Closed", StringComparison.OrdinalIgnoreCase))
            return OperationResult<SupplierContractResponse>.Fail("INVALID_STATE", "Closed records cannot be updated.", 409);

        var validation = Validate(request.Title, request.EstimatedCost, request.Status, code: null);
        if (validation is not null)
            return validation;

        existing.Title = request.Title.Trim();
        existing.ScheduledDate = request.ScheduledDate;
        existing.EstimatedCost = request.EstimatedCost;
        existing.Status = request.Status;
        existing.UpdatedAtUtc = _clock.UtcNow;
        await _repository.UpdateAsync(existing, cancellationToken);
        return OperationResult<SupplierContractResponse>.Ok(existing);
    }

    private static OperationResult<SupplierContractResponse>? Validate(
        string title,
        decimal cost,
        string status,
        string? code)
    {
        if (code is not null && string.IsNullOrWhiteSpace(code))
            return OperationResult<SupplierContractResponse>.Fail("VALIDATION_ERROR", "Code is required.", 400);
        if (string.IsNullOrWhiteSpace(title))
            return OperationResult<SupplierContractResponse>.Fail("VALIDATION_ERROR", "Title is required.", 400);
        if (cost < 0)
            return OperationResult<SupplierContractResponse>.Fail("VALIDATION_ERROR", "Cost must be >= 0.", 400);
        if (!Allowed.Contains(status))
            return OperationResult<SupplierContractResponse>.Fail("VALIDATION_ERROR", "Invalid status.", 400);
        return null;
    }
}
