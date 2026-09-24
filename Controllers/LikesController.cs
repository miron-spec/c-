using MatchApp.Backend.Controllers;
using MatchApp.Backend.Data;
using MatchApp.Backend.DTOs;
using MatchApp.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchApp.Backend.Controllers;

[Authorize, ApiController, Route("api/likes")]
public class LikesController(AppDbContext db, CurrentUserService current, MatchingService matching) : ControllerBase
{
    [HttpPost("{profileId:guid}")]
    public async Task<ActionResult<LikeResult>> Like(Guid profileId, CancellationToken ct)
    {
        var me = await current.GetProfileAsync(ct); if (me == null) return NotFound();
        var result = await matching.LikeAsync(me.Id, profileId, ct);
        return Ok(new LikeResult(result.isNewLike, result.isMutual, result.matchId));
    }

    [HttpGet("sent")]
    public async Task<IActionResult> Sent(CancellationToken ct)
    {
        var me = await current.GetProfileAsync(ct); if (me == null) return NotFound();
        var likes = await db.Likes.Where(x => x.SenderProfileId == me.Id)
            .Include(x => x.ReceiverProfile).ThenInclude(x => x.Photos)
            .Include(x => x.ReceiverProfile).ThenInclude(x => x.Interests)
            .OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
        return Ok(likes.Select(x => new { x.Id, x.CreatedAt, Profile = ProfilesController.MapDiscover(x.ReceiverProfile) }));
    }

    [HttpGet("received")]
    public async Task<IActionResult> Received(CancellationToken ct)
    {
        var me = await current.GetProfileAsync(ct); if (me == null) return NotFound();
        var likes = await db.Likes.Where(x => x.ReceiverProfileId == me.Id)
            .Include(x => x.SenderProfile).ThenInclude(x => x.Photos)
            .Include(x => x.SenderProfile).ThenInclude(x => x.Interests)
            .OrderByDescending(x => x.CreatedAt).ToListAsync(ct);
        return Ok(likes.Select(x => new { x.Id, x.CreatedAt, Profile = ProfilesController.MapDiscover(x.SenderProfile) }));
    }
}
