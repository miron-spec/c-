namespace MatchApp.Backend.Entities;

public class UserAccount
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public Guid ProfileId { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserProfile Profile { get; set; } = null!;
}
