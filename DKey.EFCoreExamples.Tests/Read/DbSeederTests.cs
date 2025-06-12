using DKey.EFCoreExamples.Infrastructure;
using DKey.EFCoreExamples.Shared;
using Microsoft.EntityFrameworkCore;

namespace DKey.EFCoreExamples.Tests.Read;

internal class DbSeederTests : SyntheticDataTest
{
    [Test]
    public async Task CanSeedDatabase()
    {
        var colors = await RepoManager.ColorRepository.GetAllAsync();
        var canvases = await RepoManager.CanvasRepository.GetAllAsync();
        Assert.IsNotNull(colors);
        Assert.IsNotNull(canvases);
        Assert.IsNotEmpty(colors);
        Assert.That(canvases.Count(), Is.EqualTo(1));
        var pixels = await RepoManager.PixelRepository.GetByCanvasIdAsync(canvases.First().Id);
        Assert.IsNotNull(pixels);
        var config = new DbConfig();
        Assert.That(pixels.Count(), Is.EqualTo(config.DefaultCanvasHeight * config.DefaultCanvasWidth));
    }
}