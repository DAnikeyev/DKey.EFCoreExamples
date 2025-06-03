using AutoMapper;
using AutoMapper.QueryableExtensions;
using DKey.EFCoreExamples.Domain;
using DKey.EFCoreExamples.Domain.Repository;
using DKey.EFCoreExamples.Shared.DTO;
using Microsoft.EntityFrameworkCore;

namespace DKey.EFCoreExamples.Model;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public SubscriptionRepository(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
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

        return _mapper.Map<SubscriptionDto>(subscription);
    }
}