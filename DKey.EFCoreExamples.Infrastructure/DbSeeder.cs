using AutoMapper;
using DKey.EFCoreExamples.Domain;
using DKey.EFCoreExamples.Domain.Repository;
using DKey.EFCoreExamples.Shared;
using DKey.EFCoreExamples.Shared.DTO;

namespace DKey.EFCoreExamples.Infrastructure;

public class DbSeeder
{
    public static async Task SeedDefaultsAsync(AppDbContext context, DbConfig config, IMapper mapper, ICanvasRepository canvasRepository)
    {
        foreach (var colorDto in config.Colors)
        {
            if (!context.Colors.Any(c => c.HexValue == colorDto.HexValue))
                await context.Colors.AddAsync(mapper.Map<Color>(colorDto));
        }

        var defaultCanvasName = config.DefaultCanvasName;
        if (!context.Canvases.Any(c => c.Name == defaultCanvasName))
        {
            var canvas = new CanvasDto()
            {
                Name = defaultCanvasName,
                Width = config.DefaultCanvasWidth,
                Height = config.DefaultCanvasHeight,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var addedDefault = await canvasRepository.TryAddCanvas(canvas, null);
            if (addedDefault is null)
            {
                throw new InvalidOperationException($"Failed to add default canvas with name {defaultCanvasName}");
            }
        }

        await context.SaveChangesAsync();
    }

    public static void SeedDefaults(AppDbContext context, DbConfig config, IMapper mapper, ICanvasRepository canvasRepository)
    {
        foreach (var colorDto in config.Colors)
        {
            if (!context.Colors.Any(c => c.HexValue == colorDto.HexValue))
                context.Colors.Add(mapper.Map<Domain.Color>(colorDto));
        }

        context.SaveChanges();

        var defaultCanvasName = config.DefaultCanvasName;
        if (!context.Canvases.Any(c => c.Name == defaultCanvasName))
        {
            var canvas = new CanvasDto()
            {
                Name = defaultCanvasName,
                Width = config.DefaultCanvasWidth,
                Height = config.DefaultCanvasHeight,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var addedDefaultTask = canvasRepository.TryAddCanvas(canvas, null);
            addedDefaultTask.Wait();
            var addedDefault = addedDefaultTask.Result;
            if (addedDefault is null)
            {
                throw new InvalidOperationException($"Failed to add default canvas with name {defaultCanvasName}");
            }
        }

        context.SaveChanges();
    }
}
