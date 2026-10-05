namespace TP01B.Contracts;

/// <summary>
/// FROZEN persistence contract. Participant implementations may use the provided
/// in-memory repository or substitute an equivalent that preserves identity semantics.
/// </summary>
public interface ISupplierContractRepository
{
    Task<SupplierContractResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
    Task AddAsync(SupplierContractResponse entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(SupplierContractResponse entity, CancellationToken cancellationToken = default);
}
