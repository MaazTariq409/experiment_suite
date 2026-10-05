namespace TP01A.Contracts;

/// <summary>
/// FROZEN persistence contract. Participant implementations may use the provided
/// in-memory repository or substitute an equivalent that preserves identity semantics.
/// </summary>
public interface IMaintenanceRecordRepository
{
    Task<MaintenanceRecordResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
    Task AddAsync(MaintenanceRecordResponse entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(MaintenanceRecordResponse entity, CancellationToken cancellationToken = default);
}
