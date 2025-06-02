using System.ComponentModel.DataAnnotations;

namespace DKey.EFCoreExamples.Model;

public class User
{
    public Guid Id { get; set; }
    
    [MaxLength(64)]
    public required string UserName { get; set; }    // Required, unique
    
    [MaxLength(64)]
    public required string Email { get; set; }      // Required, unique
    
    [MaxLength(64)]
    public string? PasswordHash { get; set; } // Null for external (Google) users
    
    [MaxLength(128)]
    public string? ProviderKey { get; set; }  // For Google: the "sub" claim; null for local users
    
    public LoginMethod LoginMethod { get; set; } // Enum
    
    public DateTime CreatedAt { get; set; }
    
    public ICollection<LoginEvent> LoginEvents { get; set; } = new List<LoginEvent>();
    public ICollection<PixelChangedEvent> PixelChangedEvents { get; set; } = new List<PixelChangedEvent>();
    public ICollection<BalanceChangedEvent> BalanceChangedEvents { get; set; } = new List<BalanceChangedEvent>();

    public ICollection<Canvas> SubscribedCanvases { get; set; } = new List<Canvas>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}

