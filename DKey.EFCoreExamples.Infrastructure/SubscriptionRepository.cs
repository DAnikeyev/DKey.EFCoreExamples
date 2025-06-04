using AutoMapper;
using AutoMapper.QueryableExtensions;
using DKey.EFCoreExamples.Domain;
using DKey.EFCoreExamples.Domain.Repository;
using DKey.EFCoreExamples.Shared;
using DKey.EFCoreExamples.Shared.DTO;
using Microsoft.EntityFrameworkCore;

namespace DKey.EFCoreExamples.Model;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IBalanceChangedEventRepository _balanceChangedEventRepository;

    public SubscriptionRepository(AppDbContext context, IMapper mapper, IBalanceChangedEventRepository balanceChangedEventRepository)
    {
        _context = context;
        _mapper = mapper;
        _balanceChangedEventRepository = balanceChangedEventRepository;
    }

    public async Task<IEnumerable<SubscriptionDto>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Subscriptions
            .AsNoTracking()
            .Where(s => s.UserId == userId)
            .ProjectTo<SubscriptionDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<IEnumerable<SubscriptionDto>> GetByCanvasIdAsync(Guid canvasId)
    {
        return await _context.Subscriptions
            .AsNoTracking()
            .Where(s => s.CanvasId == canvasId)
            .ProjectTo<SubscriptionDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<SubscriptionDto> Subscribe(Guid userId, Guid canvasId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var existing = await _context.Subscriptions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId && s.CanvasId == canvasId);

        if (existing != null)
        {
            return _mapper.Map<SubscriptionDto>(existing);
        }

        var subscription = new Subscription
        {
            UserId = userId,
            CanvasId = canvasId,
        };

        _context.Subscriptions.Add(subscription);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
        await _balanceChangedEventRepository.TryChangeBalanceAsync(userId, canvasId, 1, BalanceChangedReason.Subscription);
        return _mapper.Map<SubscriptionDto>(subscription);
    }
}

