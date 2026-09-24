namespace MatchApp.Backend.DTOs;

public record UpdatePreferencesRequest(
    int MinAge,
    int MaxAge,
    string? Gender,
    string? City,
    string? Purpose);

public record PreferencesDto(
    int MinAge,
    int MaxAge,
    string? Gender,
    string? City,
    string? Purpose);
