namespace DKey.EFCoreExamples.Shared.DTO;

public class PixelChangedEventDto
{
    public Guid Id { get; set; }
    public Guid PixelId { get; set; }
    public Guid OldOwnerUserId { get; set; }
    public Guid UserId { get; set; }
    public int OldColorId { get; set; }
    public int NewColorId { get; set; }
    public Guid? NewPrice { get; set; }
    public DateTime ChangedAt { get; set; }
}

