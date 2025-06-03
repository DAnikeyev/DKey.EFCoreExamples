namespace DKey.EFCoreExamples.Shared.DTO;

public class PixelDto
{
    public Guid Id { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int ColorId { get; set; }
    public Guid? OwnerId { get; set; }
    public Guid CanvasId { get; set; }
}

