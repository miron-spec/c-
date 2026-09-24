namespace MatchApp.Backend.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public Guid ReceiverProfileId { get; set; }
    public string Type { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public string? DataJson { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
