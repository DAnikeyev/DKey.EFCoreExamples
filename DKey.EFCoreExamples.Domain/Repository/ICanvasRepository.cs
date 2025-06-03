using DKey.EFCoreExamples.Shared.DTO;

namespace DKey.EFCoreExamples.Domain.Repository;

public interface ICanvasRepository
{
    public Task<IEnumerable<CanvasDto>> GetByUserIdAsync(Guid userId);
    public Task<CanvasDto?> GetByNameAsync(string name);
    public Task<IEnumerable<CanvasDto>> GetAllAsync();
    
    public Task<bool> TryAddCanvas(CanvasDto canvas, string? passwordHash);
}