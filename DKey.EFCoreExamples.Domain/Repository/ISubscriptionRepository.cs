using DKey.EFCoreExamples.Shared.DTO;

namespace DKey.EFCoreExamples.Domain.Repository;

public interface ISubscriptionRepository
{
    Task<IEnumerable<SubscriptionDto>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<SubscriptionDto>> GetByCanvasIdAsync(Guid canvasId);
    Task<SubscriptionDto> Subscribe(Guid userId, Guid canvasId);
}

