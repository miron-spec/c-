namespace MatchApp.Backend.Entities;

public class Match
{
    public Guid Id { get; set; }
    public Guid UserProfileId { get; set; }
    public Guid MatchedProfileId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public UserProfile UserProfile { get; set; } = null!;
    public UserProfile MatchedProfile { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
