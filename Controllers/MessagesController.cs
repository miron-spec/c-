using MatchApp.Backend.Data;
using MatchApp.Backend.DTOs;
using MatchApp.Backend.Entities;
using MatchApp.Backend.Enums;
using MatchApp.Backend.Services;
using MatchApp.Backend.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace MatchApp.Backend.Controllers;

[Authorize, ApiController, Route("api/matches/{matchId:guid}/messages")]
public class MessagesController(AppDbContext db, CurrentUserService current, FileStorageService files,
    IHubContext<ChatHub> hub) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MessageDto>>> Get(Guid matchId, [FromQuery] int take = 50,
        [FromQuery] Guid? before = null, CancellationToken ct = default)
    {
        var me = await current.GetProfileAsync(ct); if (me == null) return NotFound();
        if (!await IsMember(matchId, me.Id, ct)) return Forbid();
        take = Math.Clamp(take, 1, 100);
        var query = db.Messages.Where(x => x.MatchId == matchId);
        if (before.HasValue)
        {
            var beforeMsg = await db.Messages.FirstOrDefaultAsync(x => x.Id == before, ct);
            if (beforeMsg != null) query = query.Where(x => x.SentAt < beforeMsg.SentAt);
        }
        var list = await query.OrderByDescending(x => x.SentAt).Take(take).ToListAsync(ct);
        return Ok(list.OrderBy(x => x.SentAt).Select(Map).ToList());
    }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> Send(Guid matchId, SendMessageRequest request, CancellationToken ct)
    {
        var me = await current.GetProfileAsync(ct); if (me == null) return NotFound();
        var match = await db.Matches.FirstOrDefaultAsync(x => x.Id == matchId && x.IsActive &&
            (x.UserProfileId == me.Id || x.MatchedProfileId == me.Id), ct);
        if (match == null) return Forbid();
        if (request.Type == MessageType.Text && string.IsNullOrWhiteSpace(request.Text))
            return BadRequest(new { message = "Text is required." });
        if (request.Type != MessageType.Text && string.IsNullOrWhiteSpace(request.MediaUrl))
            return BadRequest(new { message = "MediaUrl is required for media messages." });

        var message = new Message
        {
            Id = Guid.NewGuid(), MatchId = matchId, SenderProfileId = me.Id,
            Text = request.Text, Type = request.Type, MediaUrl = request.MediaUrl,
            SentAt = DateTime.UtcNow, IsRead = false
        };
        db.Messages.Add(message); await db.SaveChangesAsync(ct);
        var dto = Map(message);
        await hub.Clients.Group($"match:{matchId}").SendAsync("message", dto, ct);
        return Ok(dto);
    }

    [HttpPost("media")]
    [RequestSizeLimit(60_000_000)]
    public async Task<ActionResult<object>> UploadMedia(Guid matchId, IFormFile file, CancellationToken ct)
    {
        var me = await current.GetProfileAsync(ct); if (me == null) return NotFound();
        if (!await IsMember(matchId, me.Id, ct)) return Forbid();
        var url = await files.SaveAsync(file, "chat", ct);
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var type = new[] { ".mp4", ".webm", ".mov" }.Contains(ext) ? MessageType.Video : MessageType.Image;
        return Ok(new { url, type = type.ToString() });
    }

    [HttpPost("read")]
    public async Task<IActionResult> MarkRead(Guid matchId, CancellationToken ct)
    {
        var me = await current.GetProfileAsync(ct); if (me == null) return NotFound();
        if (!await IsMember(matchId, me.Id, ct)) return Forbid();
        await db.Messages.Where(x => x.MatchId == matchId && x.SenderProfileId != me.Id && !x.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsRead, true), ct);
        return NoContent();
    }

    private Task<bool> IsMember(Guid matchId, Guid profileId, CancellationToken ct) =>
        db.Matches.AnyAsync(x => x.Id == matchId && x.IsActive &&
            (x.UserProfileId == profileId || x.MatchedProfileId == profileId), ct);

    private static MessageDto Map(Message x) => new(x.Id, x.MatchId, x.SenderProfileId,
        x.Text, x.Type, x.MediaUrl, x.SentAt, x.IsRead);
}
