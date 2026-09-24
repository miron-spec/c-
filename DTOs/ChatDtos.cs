using MatchApp.Backend.Enums;

namespace MatchApp.Backend.DTOs;

public record MatchDto(Guid Id, DiscoverProfileDto OtherUser, DateTime CreatedAt, bool IsActive);
public record MessageDto(Guid Id, Guid MatchId, Guid SenderProfileId, string? Text, MessageType Type, string? MediaUrl, DateTime SentAt, bool IsRead);
public record SendMessageRequest(string? Text, MessageType Type, string? MediaUrl);
public record LikeResult(bool IsNewLike, bool IsMutual, Guid? MatchId);
