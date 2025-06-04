using DKey.EFCoreExamples.Shared.DTO;

namespace DKey.EFCoreExamples.Domain.Repository;

public interface IPixelChangedEventRepository
{
    Task<IEnumerable<PixelChangedEventDto>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<PixelChangedEventDto>> GetByPixelIdAsync(Guid pixelId);
    Task<IEnumerable<PixelChangedEventDto>> GetByCanvasIdAsync(Guid canvasId, DateTime? startDate);
}

