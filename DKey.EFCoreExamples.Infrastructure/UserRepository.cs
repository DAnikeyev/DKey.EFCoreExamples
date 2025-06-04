using AutoMapper;
using AutoMapper.QueryableExtensions;
using DKey.EFCoreExamples.Domain;
using DKey.EFCoreExamples.Domain.Repository;
using DKey.EFCoreExamples.Shared;
using DKey.EFCoreExamples.Shared.DTO;
using Microsoft.EntityFrameworkCore;

namespace DKey.EFCoreExamples.Model;

public class UserRepository : IUserRepository
{
    
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IBalanceChangedEventRepository _balanceChangedEventRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly AppDefaultsConfig _defaultsConfig;

    public UserRepository(AppDbContext context, IMapper mapper, IBalanceChangedEventRepository balanceChangedEventRepository, ISubscriptionRepository subscriptionRepository, AppDefaultsConfig defaultsConfig)
    {
        _context = context;
        _mapper = mapper;
        _balanceChangedEventRepository = balanceChangedEventRepository;
        _subscriptionRepository = subscriptionRepository;
        _defaultsConfig = defaultsConfig;
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Email == email)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }

    public async Task<UserDto?> GetByUserNameAsync(string userName)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.UserName == userName)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }

    public async Task<UserDto?> AddOrUpdateUserAsync(UserDto userDto)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == userDto.Email);

        if (existingUser != null)
        {
            existingUser.UserName = userDto.UserName;
            existingUser.PasswordHashOrKey = userDto.PasswordHashOrKey;
            existingUser.LoginMethod = userDto.LoginMethod;
            _context.Users.Update(existingUser);
            await transaction.CommitAsync();
        }
        else
        {
            var newUser = _mapper.Map<User>(userDto);
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            await _subscriptionRepository.Subscribe(newUser.Id, _defaultsConfig.DefaultCanvasId);
        }


        return await GetByEmailAsync(userDto.Email);
    }

    public async Task<UserDto?> DeleteUserAsync(UserDto userDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userDto.Id);
        if (user == null)
            return null;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return _mapper.Map<UserDto>(user);
    }

    public Task<UserDto?> TryLogin(UserDto userDto, PasswordDto passwordDto)
    {
        return _context.Users
        .AsNoTracking()
        .Where(u => u.Email == userDto.Email && u.PasswordHashOrKey == passwordDto.PasswordHashOrKey)
        .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
        .FirstOrDefaultAsync();
    }
}

