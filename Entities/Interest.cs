namespace MatchApp.Backend.Entities;

public class Interest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<UserProfile> UserProfiles { get; set; } = new List<UserProfile>();
}
