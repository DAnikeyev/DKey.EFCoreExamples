using AutoMapper;
using AutoMapper.QueryableExtensions;
using DKey.EFCoreExamples.Shared.DTO;
using DKey.EFCoreExamples.Domain;
using DKey.EFCoreExamples.Domain.Repository;
using Microsoft.EntityFrameworkCore;

namespace DKey.EFCoreExamples.Model;

public class BalanceChangedEventRepository : IBalanceChangedEventRepository
{
    private IMapper _mapper;
    private AppDbContext _context;
    
    public BalanceChangedEventRepository(AppDbContext context, IMapper mapper)
    {
        _mapper = mapper;
        _context = context;
    }
    
    public async Task<IEnumerable<BalanceChangedEventDto>> GetByUserIdAsync(Guid userId)
    {
        return await _context.BalanceChangedEvents.AsNoTracking()
            .Where(e => e.UserId == userId)
            .ProjectTo<BalanceChangedEventDto>(_mapper.ConfigurationProvider)
            .ToListAsync()
            .ContinueWith(t => t.Result.AsEnumerable());
    }

    public async Task<BalanceChangedEventDto?> TryChangeBalanceAsync(Guid userId, Guid canvasId, long delta, string? reason)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var lastEntry = await _context.BalanceChangedEvents
                .Where(e => e.UserId == userId && e.CanvasId == canvasId)
                .OrderByDescending(e => e.ChangedAt)
                .FirstOrDefaultAsync();

            var newBalance = lastEntry?.NewBalance + delta ?? delta;
            if (newBalance < 0)
            {
                return null;
            }

            var newEvent = new BalanceChangedEvent
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CanvasId = canvasId,
                ChangedAt = DateTime.UtcNow,
                NewBalance = newBalance,
                OldBalance = lastEntry?.NewBalance ?? 0,
                Reason = reason,
            };
            await _context.BalanceChangedEvents.AddAsync(newEvent);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return _mapper.Map<BalanceChangedEventDto>(newEvent);
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            return null;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
    
}