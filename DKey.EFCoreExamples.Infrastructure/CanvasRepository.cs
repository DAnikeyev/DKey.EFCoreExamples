using AutoMapper;
using AutoMapper.QueryableExtensions;
using DKey.EFCoreExamples.Domain;
using DKey.EFCoreExamples.Domain.Repository;
using DKey.EFCoreExamples.Shared.DTO;
using Microsoft.EntityFrameworkCore;
using NLog;

namespace DKey.EFCoreExamples.Model;

public class CanvasRepository : ICanvasRepository
{
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    
    public CanvasRepository(AppDbContext context, IMapper mapper)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task<IEnumerable<CanvasDto>> GetByUserIdAsync(Guid userId)
    {
        var subs = _context.Subscriptions.AsNoTracking().Where(x => x.UserId == userId).Select(x => x.CanvasId).ToHashSet();
        return await _context.Canvases.AsNoTracking().Where(c => subs.Contains(c.Id)).ProjectTo<CanvasDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    public async Task<CanvasDto?> GetByNameAsync(string name)
    {
        return await _context.Canvases.AsNoTracking()
            .Where(c => c.Name == name)
            .ProjectTo<CanvasDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<CanvasDto>> GetAllAsync()
    {
        return await _context.Canvases.AsNoTracking().ProjectTo<CanvasDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    public async Task<bool> TryAddCanvas(CanvasDto canvas, string? passwordHash)
    {
        var newCanvas = _mapper.Map<Canvas>(canvas);
        newCanvas.Id = Guid.NewGuid();
        newCanvas.PasswordHash = passwordHash;
        var now = DateTime.UtcNow;
        newCanvas.CreatedAt = now;
        newCanvas.UpdatedAt = now;
        // Generate pixels for the canvas
        var pixels = new List<Pixel>();
        for (int x = 0; x < newCanvas.Width; x++)
        {
            for (int y = 0; y < newCanvas.Height; y++)
            {
                pixels.Add(new Pixel
                {
                    Id = Guid.NewGuid(),
                    X = x,
                    Y = y,
                    CanvasId = newCanvas.Id,
                    ColorId = 0, // Default color, adjust as needed
                    Price = 0 // Default price, adjust as needed
                });
            }
        }
        try
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            await _context.Canvases.AddAsync(newCanvas);
            await _context.Pixels.AddRangeAsync(pixels);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (DbUpdateException ex)
        {
            Logger.Error(ex, "Error adding canvas or pixels");
            return false;
        }
    }
}

