using Enterprise.Shared.Results;

namespace TP01A.Contracts;

/// <summary>
/// FROZEN CONTRACT — participants implement this interface; do not rename members.
/// Evaluation tests compile only against this contract.
/// </summary>
public interface IMaintenanceRecordService
{
    Task<OperationResult<MaintenanceRecordResponse>> CreateAsync(MaintenanceRecordCreateRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult<MaintenanceRecordResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OperationResult<MaintenanceRecordResponse>> UpdateAsync(Guid id, MaintenanceRecordUpdateRequest request, CancellationToken cancellationToken = default);
}
