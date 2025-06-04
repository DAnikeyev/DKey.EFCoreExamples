using AutoMapper;
using AutoMapper.QueryableExtensions;
using DKey.EFCoreExamples.Domain.Repository;
using DKey.EFCoreExamples.Shared.DTO;
using Microsoft.EntityFrameworkCore;

namespace DKey.EFCoreExamples.Model;

public class PixelChangedEventRepository : IPixelChangedEventRepository
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public PixelChangedEventRepository(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PixelChangedEventDto>> GetByUserIdAsync(Guid userId)
    {
        return await _context.PixelChangedEvents
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .ProjectTo<PixelChangedEventDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    public async Task<IEnumerable<PixelChangedEventDto>> GetByPixelIdAsync(Guid pixelId)
    {
        return await _context.PixelChangedEvents
            .AsNoTracking()
            .Where(e => e.PixelId == pixelId)
            .ProjectTo<PixelChangedEventDto>(_mapper.ConfigurationProvider).ToListAsync();
    }

    public async Task<IEnumerable<PixelChangedEventDto>> GetByCanvasIdAsync(Guid canvasId, DateTime? startDate)
    {
        var query = _context.PixelChangedEvents
            .AsNoTracking()
            .Where(e => e.Pixel.CanvasId == canvasId);

        if (startDate.HasValue)
        {
            query = query.Where(e => e.ChangedAt >= startDate.Value);
        }

        return await query
            .ProjectTo<PixelChangedEventDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }
}