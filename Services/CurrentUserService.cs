using System.Security.Claims;
using MatchApp.Backend.Data;
using MatchApp.Backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace MatchApp.Backend.Services;

public class CurrentUserService(IHttpContextAccessor accessor, AppDbContext db)
{
    public string UserId => accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();

    public Task<UserProfile?> GetProfileAsync(CancellationToken ct = default) =>
        db.UserProfiles.Include(x => x.Photos).Include(x => x.Interests)
            .FirstOrDefaultAsync(x => x.UserId == UserId, ct);
}
