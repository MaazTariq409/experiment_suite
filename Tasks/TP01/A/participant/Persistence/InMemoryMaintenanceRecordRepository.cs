using TP01A.Contracts;

namespace TP01A.Participant.Persistence;

public sealed class InMemoryMaintenanceRecordRepository : IMaintenanceRecordRepository
{
    private readonly Dictionary<Guid, MaintenanceRecordResponse> _store = new();

    public Task<MaintenanceRecordResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var value);
        return Task.FromResult(value);
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        var exists = _store.Values.Any(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(exists);
    }

    public Task AddAsync(MaintenanceRecordResponse entity, CancellationToken cancellationToken = default)
    {
        _store[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(MaintenanceRecordResponse entity, CancellationToken cancellationToken = default)
    {
        _store[entity.Id] = entity;
        return Task.CompletedTask;
    }
}
