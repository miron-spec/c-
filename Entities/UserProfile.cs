namespace MatchApp.Backend.Entities;

public class UserProfile
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public int Age { get; set; }
    public string? Gender { get; set; }
    public string? City { get; set; }
    public string? Bio { get; set; }
    public bool HideBio { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool HasSeenChatTutorial { get; set; }

    public ICollection<ProfilePhoto> Photos { get; set; } = new List<ProfilePhoto>();
    public ICollection<Interest> Interests { get; set; } = new List<Interest>();
    public SearchPreference? SearchPreference { get; set; }
}
