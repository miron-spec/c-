namespace MatchApp.Backend.DTOs;

public record RegisterRequest(string Email, string Password);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string UserId, string Token, Guid ProfileId);
