using MatchApp.Backend.Controllers;
using MatchApp.Backend.Data;
using MatchApp.Backend.DTOs;
using MatchApp.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchApp.Backend.Controllers;

[Authorize, ApiController, Route("api/matches")]
public class MatchesController(AppDbContext db, CurrentUserService current) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MatchDto>>> Get(CancellationToken ct)
    {
        var me = await current.GetProfileAsync(ct); if (me == null) return NotFound();
        var matches = await db.Matches.Where(x => x.IsActive &&
                (x.UserProfileId == me.Id || x.MatchedProfileId == me.Id))
            .Include(x => x.UserProfile).ThenInclude(x => x.Photos)
            .Include(x => x.UserProfile).ThenInclude(x => x.Interests)
            .Include(x => x.MatchedProfile).ThenInclude(x => x.Photos)
            .Include(x => x.MatchedProfile).ThenInclude(x => x.Interests)
            .OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
        return Ok(matches.Select(x => new MatchDto(x.Id,
            ProfilesController.MapDiscover(x.UserProfileId == me.Id ? x.MatchedProfile : x.UserProfile),
            x.CreatedAt, x.IsActive)).ToList());
    }

    [HttpGet("{matchId:guid}")]
    public async Task<ActionResult<MatchDto>> Get(Guid matchId, CancellationToken ct)
    {
        var me = await current.GetProfileAsync(ct); if (me == null) return NotFound();
        var m = await db.Matches.Where(x => x.Id == matchId && x.IsActive &&
                (x.UserProfileId == me.Id || x.MatchedProfileId == me.Id))
            .Include(x => x.UserProfile).ThenInclude(x => x.Photos)
            .Include(x => x.UserProfile).ThenInclude(x => x.Interests)
            .Include(x => x.MatchedProfile).ThenInclude(x => x.Photos)
            .Include(x => x.MatchedProfile).ThenInclude(x => x.Interests)
            .FirstOrDefaultAsync(ct);
        if (m == null) return NotFound();
        var other = m.UserProfileId == me.Id ? m.MatchedProfile : m.UserProfile;
        return Ok(new MatchDto(m.Id, ProfilesController.MapDiscover(other), m.CreatedAt, m.IsActive));
    }
}
