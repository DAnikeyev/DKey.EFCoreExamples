using DKey.EFCoreExamples.Shared.DTO;

namespace DKey.EFCoreExamples.Domain.Repository;

public interface ILoginEventRepository
{
    public Task<IEnumerable<LoginEventDto>> GetByUserIdAsync(Guid userId);
    public Task<bool> AddLoginEvent(LoginEventDto loginEvent);
}

