using AutoMapper;
using DKey.EFCoreExamples.Domain.Repository;
using DKey.EFCoreExamples.Shared.DTO;

namespace DKey.EFCoreExamples.Model;

public class PixelRepository : IPixelRepository
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public PixelRepository(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PixelDto>> GetByCanvasIdAsync(Guid canvasId)
    {
    }

    public async Task<IEnumerable<PixelDto>> GetByOwnerIdAsync(Guid ownerId)
    {
    }

    public async Task<PixelDto> TryChangePixelAsync(Guid ownerId, PixelDto pixel)
    {
    }
}