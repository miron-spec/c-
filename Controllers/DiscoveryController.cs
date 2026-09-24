using MatchApp.Backend.Controllers;
using MatchApp.Backend.Data;
using MatchApp.Backend.DTOs;
using MatchApp.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchApp.Backend.Controllers;

[Authorize, ApiController, Route("api/discovery")]
public class DiscoveryController(AppDbContext db, CurrentUserService current) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<DiscoverProfileDto>>> Get([FromQuery] int take = 20, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 50);
        var me = await current.GetProfileAsync(ct); if (me == null) return NotFound();
        var pref = await db.SearchPreferences.FirstOrDefaultAsync(x => x.UserProfileId == me.Id, ct);
        var likedIds = await db.Likes.Where(x => x.SenderProfileId == me.Id).Select(x => x.ReceiverProfileId).ToListAsync(ct);
        var query = db.UserProfiles.Include(x => x.Photos).Include(x => x.Interests)
            .Where(x => x.Id != me.Id && !likedIds.Contains(x.Id));

        if (pref != null)
        {
            query = query.Where(x => x.Age >= pref.MinAge && x.Age <= pref.MaxAge);
            if (!string.IsNullOrWhiteSpace(pref.Gender)) query = query.Where(x => x.Gender == pref.Gender);
            if (!string.IsNullOrWhiteSpace(pref.City)) query = query.Where(x => x.City == pref.City);
        }
        var result = await query.OrderByDescending(x => x.UpdatedAt).Take(take).ToListAsync(ct);
        return Ok(result.Select(ProfilesController.MapDiscover).ToList());
    }
}
