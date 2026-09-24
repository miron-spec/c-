namespace MatchApp.Backend.DTOs;

public record UpdateProfileRequest(
    string Name,
    int Age,
    string? Gender,
    string? City,
    string? Bio,
    bool HideBio);

public record ProfilePhotoDto(Guid Id, string Url, int SortOrder);

public record ProfileDto(
    Guid Id,
    string Name,
    int Age,
    string? Gender,
    string? City,
    string? Bio,
    bool HideBio,
    List<ProfilePhotoDto> Photos,
    List<string> Interests,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record DiscoverProfileDto(
    Guid Id,
    string Name,
    int Age,
    string? Gender,
    string? City,
    string? Bio,
    bool HideBio,
    List<ProfilePhotoDto> Photos,
    List<string> Interests);
