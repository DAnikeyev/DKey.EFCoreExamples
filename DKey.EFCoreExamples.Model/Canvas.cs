using System.ComponentModel.DataAnnotations;

namespace DKey.EFCoreExamples.Model;

public class Canvas
{
    public Guid Id { get; set; }
    
    [MaxLength(128)]
    public required string Name { get; set; }

    [Range(1,1920)]
    public int Width { get; set; }
    
    [Range(1, 1080)]
    public int Height { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    [MaxLength(128)]
    public string? PasswordHash { get; set; }  // Null if no password, hashed if protected
    
    public ICollection<Pixel> Pixels { get; set; }

    public ICollection<User> Subscribers { get; set; } = new List<User>();

    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}
