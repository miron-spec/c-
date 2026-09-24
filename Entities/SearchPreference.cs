namespace MatchApp.Backend.Entities;

public class SearchPreference
{
    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public int MinAge { get; set; } = 18;
    public int MaxAge { get; set; } = 100;
    public string? Gender { get; set; }
    public string? City { get; set; }
    public string? Purpose { get; set; }
    public UserProfile UserProfile { get; set; } = null!;
}
