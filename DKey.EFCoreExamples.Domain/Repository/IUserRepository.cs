using DKey.EFCoreExamples.Shared.DTO;

namespace DKey.EFCoreExamples.Domain.Repository;

public interface IUserRepository
{
    public Task<UserDto?> GetByEmailAsync(string email);
    public Task<UserDto?> GetByUserNameAsync(string userName);
    public Task<UserDto?> GetByIdAsync(Guid id);
    public Task<UserDto?> AddOrUpdateUserAsync(UserDto userDto);
    
    public Task<UserDto?> DeleteUserAsync(UserDto userDto);
    public Task<UserDto?> TryLogin(UserDto userDto, PasswordDto passwordDto);
}