using MatchApp.Backend.Data;
using MatchApp.Backend.DTOs;
using MatchApp.Backend.Entities;
using MatchApp.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchApp.Backend.Controllers;

[Authorize, ApiController, Route("api/profiles")]
public class ProfilesController(AppDbContext db, CurrentUserService current, FileStorageService files) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<ProfileDto>> GetMe(CancellationToken ct)
    {
        var p = await current.GetProfileAsync(ct);
        return p == null ? NotFound() : Ok(Map(p));
    }

    [HttpPut("me")]
    public async Task<ActionResult<ProfileDto>> Update(UpdateProfileRequest request, CancellationToken ct)
    {
        if (request.Age is < 18 or > 100) return BadRequest(new { message = "Age must be between 18 and 100." });
        if (string.IsNullOrWhiteSpace(request.Name)) return BadRequest(new { message = "Name is required." });
        var p = await current.GetProfileAsync(ct);
        if (p == null) return NotFound();
        p.Name = request.Name.Trim(); p.Age = request.Age; p.Gender = request.Gender?.Trim();
        p.City = request.City?.Trim(); p.Bio = request.Bio; p.HideBio = request.HideBio;
        p.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(Map(p));
    }

    [HttpPost("me/photos")]
    [RequestSizeLimit(35_000_000)]
    public async Task<ActionResult<ProfilePhotoDto>> UploadPhoto(IFormFile file, CancellationToken ct)
    {
        var p = await current.GetProfileAsync(ct);
        if (p == null) return NotFound();
        if (p.Photos.Count >= 3) return BadRequest(new { message = "Maximum 3 profile photos." });
        var url = await files.SaveAsync(file, "profile", ct);
        var photo = new ProfilePhoto
        {
            Id = Guid.NewGuid(), UserProfileId = p.Id, Url = url,
            SortOrder = p.Photos.Count, CreatedAt = DateTime.UtcNow
        };
        db.ProfilePhotos.Add(photo); await db.SaveChangesAsync(ct);
        return Ok(new ProfilePhotoDto(photo.Id, photo.Url, photo.SortOrder));
    }

    [HttpDelete("me/photos/{photoId:guid}")]
    public async Task<IActionResult> DeletePhoto(Guid photoId, CancellationToken ct)
    {
        var p = await current.GetProfileAsync(ct);
        if (p == null) return NotFound();
        var photo = p.Photos.FirstOrDefault(x => x.Id == photoId);
        if (photo == null) return NotFound();
        db.ProfilePhotos.Remove(photo);
        await db.SaveChangesAsync(ct);
        var remaining = await db.ProfilePhotos.Where(x => x.UserProfileId == p.Id)
            .OrderBy(x => x.SortOrder).ToListAsync(ct);
        for (var i = 0; i < remaining.Count; i++) remaining[i].SortOrder = i;
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpPost("me/tutorial/seen")]
    public async Task<IActionResult> MarkChatTutorialSeen(CancellationToken ct)
    {
        var p = await current.GetProfileAsync(ct); if (p == null) return NotFound();
        p.HasSeenChatTutorial = true; await db.SaveChangesAsync(ct); return NoContent();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DiscoverProfileDto>> Get(Guid id, CancellationToken ct)
    {
        var p = await db.UserProfiles.Include(x => x.Photos).Include(x => x.Interests)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        return p == null ? NotFound() : Ok(MapDiscover(p));
    }

    private static ProfileDto Map(UserProfile p) => new(p.Id, p.Name, p.Age, p.Gender, p.City,
        p.HideBio ? null : p.Bio, p.HideBio,
        p.Photos.OrderBy(x => x.SortOrder).Select(x => new ProfilePhotoDto(x.Id, x.Url, x.SortOrder)).ToList(),
        p.Interests.OrderBy(x => x.Name).Select(x => x.Name).ToList(), p.CreatedAt, p.UpdatedAt);

    public static DiscoverProfileDto MapDiscover(UserProfile p) => new(p.Id, p.Name, p.Age, p.Gender, p.City,
        p.HideBio ? null : p.Bio, p.HideBio,
        p.Photos.OrderBy(x => x.SortOrder).Select(x => new ProfilePhotoDto(x.Id, x.Url, x.SortOrder)).ToList(),
        p.Interests.OrderBy(x => x.Name).Select(x => x.Name).ToList());
}
