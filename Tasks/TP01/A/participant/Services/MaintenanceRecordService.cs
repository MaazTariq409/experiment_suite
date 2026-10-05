using Enterprise.Shared.Results;
using Enterprise.Shared.Time;
using TP01A.Contracts;

namespace TP01A.Participant.Services;

/// <summary>
/// PARTICIPANT IMPLEMENTATION AREA.
/// Replace NotImplementedException bodies with full business logic.
/// </summary>
public sealed class MaintenanceRecordService : IMaintenanceRecordService
{
    private readonly IMaintenanceRecordRepository _repository;
    private readonly IClock _clock;

    public MaintenanceRecordService(IMaintenanceRecordRepository repository, IClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public Task<OperationResult<MaintenanceRecordResponse>> CreateAsync(MaintenanceRecordCreateRequest request, CancellationToken cancellationToken = default)
    {
        // TODO: validate required fields, unique Code, cost >= 0, status in allowed set; persist; return 201
        throw new NotImplementedException("Implement CreateAsync for TP01-A.");
    }

    public Task<OperationResult<MaintenanceRecordResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // TODO: return 404 when missing; otherwise 200 with entity
        throw new NotImplementedException("Implement GetByIdAsync for TP01-A.");
    }

    public Task<OperationResult<MaintenanceRecordResponse>> UpdateAsync(Guid id, MaintenanceRecordUpdateRequest request, CancellationToken cancellationToken = default)
    {
        // TODO: validate; reject Closed status updates; return 404 when missing; return 200 on success
        throw new NotImplementedException("Implement UpdateAsync for TP01-A.");
    }
}
