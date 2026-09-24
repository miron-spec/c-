using MatchApp.Backend.Enums;

namespace MatchApp.Backend.Entities;

public class Message
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public Guid SenderProfileId { get; set; }
    public string? Text { get; set; }
    public MessageType Type { get; set; }
    public string? MediaUrl { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
    public Match Match { get; set; } = null!;
    public UserProfile SenderProfile { get; set; } = null!;
}
