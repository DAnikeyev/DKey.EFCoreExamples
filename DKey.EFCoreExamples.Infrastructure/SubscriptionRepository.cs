using AutoMapper;
using AutoMapper.QueryableExtensions;
using DKey.EFCoreExamples.Domain;
using DKey.EFCoreExamples.Domain.Repository;
using DKey.EFCoreExamples.Shared;
using DKey.EFCoreExamples.Shared.DTO;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace DKey.EFCoreExamples.Infrastructure;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IBalanceChangedEventRepository _balanceChangedEventRepository;
    private readonly ILogger _logger = LogManager.GetCurrentClassLogger();

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

    public async Task<SubscriptionDto> Subscribe(Guid userId, Guid canvasId, string? passwordHash)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var existing = await _context.Subscriptions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId && s.CanvasId == canvasId);

            var canavas = await _context.Canvases
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == canvasId);
            if (canavas == null)
            {
                _logger.Warn("Canvas not found. userId={UserId}, canvasId={CanvasId}", userId, canvasId);
                throw new InvalidOperationException("Canvas not found.");
            }
            if (canavas.PasswordHash != passwordHash)
            {
                _logger.Warn("Invalid password for the canvas. userId={UserId}, canvasId={CanvasId}", userId, canvasId);
                throw new InvalidOperationException("Invalid password for the canvas.");
            }
            if (existing != null)
            {
                _logger.Info("User already subscribed. userId={UserId}, canvasId={CanvasId}", userId, canvasId);
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
            _logger.Info("User subscribed successfully. userId={UserId}, canvasId={CanvasId}", userId, canvasId);
            return _mapper.Map<SubscriptionDto>(subscription);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.Error(ex, "Error subscribing user. userId={UserId}, canvasId={CanvasId}", userId, canvasId);
            throw;
        }
    }
}

