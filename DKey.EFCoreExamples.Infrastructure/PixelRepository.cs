using AutoMapper;
using DKey.EFCoreExamples.Domain;
using DKey.EFCoreExamples.Domain.Repository;
using DKey.EFCoreExamples.Shared;
using DKey.EFCoreExamples.Shared.DTO;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace DKey.EFCoreExamples.Model;

public class PixelRepository : IPixelRepository
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IBalanceChangedEventRepository _balanceChangedEventRepository;
    private static readonly ILogger Logger = NLog.LogManager.GetCurrentClassLogger();

    public PixelRepository(AppDbContext context, IMapper mapper, IBalanceChangedEventRepository balanceChangedEventRepository)
    {
        _context = context;
        _mapper = mapper;
        _balanceChangedEventRepository = balanceChangedEventRepository;
    }

    public async Task<IEnumerable<PixelDto>> GetByCanvasIdAsync(Guid canvasId)
    {
        var pixels = await _context.Pixels
            .Where(p => p.CanvasId == canvasId)
            .Select(p => _mapper.Map<PixelDto>(p))
            .ToListAsync();
        return pixels;
    }

    public async Task<IEnumerable<PixelDto>> GetByOwnerIdAsync(Guid ownerId)
    {
        var pixels = await _context.Pixels
            .Where(p => p.OwnerId == ownerId)
            .Select(p => _mapper.Map<PixelDto>(p))
            .ToListAsync();
        return pixels;
    }

    public async Task<PixelDto?> TryChangePixelAsync(Guid ownerId, PixelDto pixel)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var existingPixel = await _context.Pixels.AsNoTracking()
                .FirstAsync(p => p.Id == pixel.Id);
            var price = existingPixel.Price + 1;
            var balanceEvent = await _context.BalanceChangedEvents.AsNoTracking().Where(x => x.UserId == ownerId).OrderByDescending(x => x.ChangedAt)
                .FirstOrDefaultAsync();
            var paid = pixel.Price;
            if (balanceEvent == null)
            {
                throw new InvalidOperationException("No balance event found for the user.");
            }
            if (paid < price)
            {
                throw new InvalidOperationException("Insufficient payment to change the pixel.");
            }
            if (balanceEvent.NewBalance < paid)
            {
                throw new InvalidOperationException("Insufficient balance to change the pixel.");
            }

            var balanceUpdate = await _balanceChangedEventRepository.TryChangeBalanceAsync(ownerId, pixel.CanvasId, -paid, BalanceChangedReason.PixelPayment);
            
            if (balanceUpdate == null)
            {
                throw new InvalidOperationException("Failed to update balance.");
            }
            var newPixel = _mapper.Map<Pixel>(pixel);
            _context.Pixels.Update(newPixel);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return _mapper.Map<PixelDto>(newPixel);
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            await transaction.RollbackAsync();
            return null;
        }
    }
}