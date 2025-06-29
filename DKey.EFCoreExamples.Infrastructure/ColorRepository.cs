using AutoMapper;
using AutoMapper.QueryableExtensions;
using DKey.EFCoreExamples.Domain.Repository;
using DKey.EFCoreExamples.Shared.DTO;
using Microsoft.EntityFrameworkCore;

namespace DKey.EFCoreExamples.Infrastructure;

public class ColorRepository : IColorRepository
{
    private AppDbContext _context;
    private IMapper _mapper;

    public ColorRepository(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ColorDto>> GetAllAsync()
    {
        return await _context.Colors.ProjectTo<ColorDto>(_mapper.ConfigurationProvider).ToListAsync();
    }
}