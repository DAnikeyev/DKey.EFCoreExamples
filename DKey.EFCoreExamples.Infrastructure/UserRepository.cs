using AutoMapper;
using AutoMapper.QueryableExtensions;
using DKey.EFCoreExamples.Domain;
using DKey.EFCoreExamples.Domain.Repository;
using DKey.EFCoreExamples.Shared;
using DKey.EFCoreExamples.Shared.DTO;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace DKey.EFCoreExamples.Infrastructure;

public class UserRepository : IUserRepository
{
    
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IBalanceChangedEventRepository _balanceChangedEventRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly DbConfig _defaultsConfig;
    private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    public UserRepository(AppDbContext context, IMapper mapper, IBalanceChangedEventRepository balanceChangedEventRepository, ISubscriptionRepository subscriptionRepository, DbConfig defaultsConfig)
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

    public async Task<UserDto?> AddOrUpdateUserAsync(UserDto userDto, PasswordDto passwordDto)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var existingUser = (userDto.Id != Guid.Empty) ?
                await _context.Users.FirstOrDefaultAsync(u => u.Id == userDto.Id) : 
                await _context.Users
                .FirstOrDefaultAsync(u => u.Email == userDto.Email);

            if (existingUser != null)
            {
                existingUser.Email = userDto.Email;
                existingUser.UserName = userDto.UserName;
                existingUser.PasswordHashOrKey = passwordDto.PasswordHashOrKey ?? existingUser.PasswordHashOrKey;
                existingUser.LoginMethod = passwordDto.LoginMethod;
                _context.Users.Update(existingUser);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            else
            {
                if(passwordDto.PasswordHashOrKey == null)
                {
                    throw new InvalidDataException($"Password hash or key is required for new user: {userDto.Email}");
                }
                var newUser = _mapper.Map<User>(userDto);
                newUser.PasswordHashOrKey = passwordDto.PasswordHashOrKey;
                newUser.LoginMethod = passwordDto.LoginMethod;
                newUser.Id = Guid.NewGuid();
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                var mainCanvas = _context.Canvases.AsNoTracking().FirstOrDefault(x => x.Name == _defaultsConfig.DefaultCanvasName);
                if (mainCanvas == null)
                {
                    throw new InvalidOperationException($"Main canvas with name '{_defaultsConfig.DefaultCanvasName}' not found.");
                }

                await _subscriptionRepository.Subscribe(newUser.Id, mainCanvas.Id, null);
            }

            var userInDb = await GetByEmailAsync(userDto.Email);
            if (userInDb == null)
            {
                throw new InvalidOperationException($"Could not find user after adding/updating: {userDto.Email}");
            }
            return await GetByEmailAsync(userDto.Email);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in AddOrUpdateUserAsync for user: {Email}", userDto.Email);
            await transaction.RollbackAsync();
            return null;
        }
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
        .Where(u => u.Email == userDto.Email && u.PasswordHashOrKey == passwordDto.PasswordHashOrKey && u.LoginMethod == passwordDto.LoginMethod)
        .ProjectTo<UserDto>(_mapper.ConfigurationProvider)
        .FirstOrDefaultAsync();
    }
}

