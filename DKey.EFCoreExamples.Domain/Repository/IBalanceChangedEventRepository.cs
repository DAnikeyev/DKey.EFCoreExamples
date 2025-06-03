using DKey.EFCoreExamples.Shared.DTO;

namespace DKey.EFCoreExamples.Domain.Repository;

public interface IBalanceChangedEventRepository
{
    public Task<IEnumerable<BalanceChangedEventDto>> GetByUserIdAsync(Guid userId);
    public Task<BalanceChangedEventDto?> TryChangeBalanceAsync(Guid userId, Guid canvasId, long delta, string? reason);
}