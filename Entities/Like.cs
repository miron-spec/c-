namespace MatchApp.Backend.Entities;

public class Like
{
    public Guid Id { get; set; }
    public Guid SenderProfileId { get; set; }
    public Guid ReceiverProfileId { get; set; }
    public DateTime CreatedAt { get; set; }
    public UserProfile SenderProfile { get; set; } = null!;
    public UserProfile ReceiverProfile { get; set; } = null!;
}
