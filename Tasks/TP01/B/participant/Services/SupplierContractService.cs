using Enterprise.Shared.Results;
using Enterprise.Shared.Time;
using TP01B.Contracts;

namespace TP01B.Participant.Services;

/// <summary>
/// PARTICIPANT IMPLEMENTATION AREA.
/// Replace NotImplementedException bodies with full business logic.
/// </summary>
public sealed class SupplierContractService : ISupplierContractService
{
    private readonly ISupplierContractRepository _repository;
    private readonly IClock _clock;

    public SupplierContractService(ISupplierContractRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public Task<OperationResult<SupplierContractResponse>> CreateAsync(SupplierContractCreateRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Implement CreateAsync for TP01-B.");
    }

    public Task<OperationResult<SupplierContractResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Implement GetByIdAsync for TP01-B.");
    }

    public Task<OperationResult<SupplierContractResponse>> UpdateAsync(Guid id, SupplierContractUpdateRequest request, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Implement UpdateAsync for TP01-B.");
    }
}
