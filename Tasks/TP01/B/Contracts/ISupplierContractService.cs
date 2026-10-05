using Enterprise.Shared.Results;

namespace TP01B.Contracts;

/// <summary>
/// FROZEN CONTRACT — participants implement this interface; do not rename members.
/// Evaluation tests compile only against this contract.
/// </summary>
public interface ISupplierContractService
{
    Task<OperationResult<SupplierContractResponse>> CreateAsync(SupplierContractCreateRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult<SupplierContractResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OperationResult<SupplierContractResponse>> UpdateAsync(Guid id, SupplierContractUpdateRequest request, CancellationToken cancellationToken = default);
}
