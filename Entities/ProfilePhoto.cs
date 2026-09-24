namespace MatchApp.Backend.Entities;

public class ProfilePhoto
{
    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public string Url { get; set; } = null!;
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserProfile UserProfile { get; set; } = null!;
}
