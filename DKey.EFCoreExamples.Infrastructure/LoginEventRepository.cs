using AutoMapper;
using DKey.EFCoreExamples.Domain;
using DKey.EFCoreExamples.Domain.Repository;
using DKey.EFCoreExamples.Shared.DTO;
using Microsoft.EntityFrameworkCore;

namespace DKey.EFCoreExamples.Infrastructure;

public class LoginEventRepository: ILoginEventRepository
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public LoginEventRepository(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<LoginEventDto>> GetByUserIdAsync(Guid userId)
    {
        var events = await _context.LoginEvents
            .Where(e => e.UserId == userId)
            .Select(e => _mapper.Map<LoginEventDto>(e))
            .ToListAsync();
        return events;
    }

    public async Task<bool> AddLoginEvent(LoginEventDto loginEventDto)
    {
        var loginEvent = _mapper.Map<LoginEvent>(loginEventDto);
        loginEvent.LoggedInAt = DateTime.UtcNow;
        await _context.LoginEvents.AddAsync(loginEvent);
        await _context.SaveChangesAsync();
        return true;
    }
}