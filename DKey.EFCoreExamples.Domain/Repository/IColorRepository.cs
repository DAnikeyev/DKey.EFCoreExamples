using DKey.EFCoreExamples.Shared.DTO;

namespace DKey.EFCoreExamples.Domain.Repository;

public interface IColorRepository
{
    public Task<IEnumerable<ColorDto>> GetAllColorsAsync();
}

