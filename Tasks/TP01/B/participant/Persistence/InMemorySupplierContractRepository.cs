using TP01B.Contracts;

namespace TP01B.Participant.Persistence;

public sealed class InMemorySupplierContractRepository : ISupplierContractRepository
{
    private readonly Dictionary<Guid, SupplierContractResponse> _store = new();

    public Task<SupplierContractResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var value);
        return Task.FromResult(value);
    }

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        var exists = _store.Values.Any(x => string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(exists);
    }

    public Task AddAsync(SupplierContractResponse entity, CancellationToken cancellationToken = default)
    {
        _store[entity.Id] = entity;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(SupplierContractResponse entity, CancellationToken cancellationToken = default)
    {
        _store[entity.Id] = entity;
        return Task.CompletedTask;
    }
}
