using MatchApp.Backend.Data;
using MatchApp.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchApp.Backend.Controllers;

[Authorize, ApiController, Route("api/notifications")]
public class NotificationsController(AppDbContext db, CurrentUserService current) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var me = await current.GetProfileAsync(ct); if (me == null) return NotFound();
        return Ok(await db.Notifications.Where(x => x.ReceiverProfileId == me.Id)
            .OrderByDescending(x => x.CreatedAt).Take(100).ToListAsync(ct));
    }

    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> Read(Guid id, CancellationToken ct)
    {
        var me = await current.GetProfileAsync(ct); if (me == null) return NotFound();
        var n = await db.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.ReceiverProfileId == me.Id, ct);
        if (n == null) return NotFound(); n.IsRead = true; await db.SaveChangesAsync(ct); return NoContent();
    }
}
