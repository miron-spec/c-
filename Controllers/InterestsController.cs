using MatchApp.Backend.Data;
using MatchApp.Backend.Entities;
using MatchApp.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchApp.Backend.Controllers;

[Authorize, ApiController, Route("api/interests")]
public class InterestsController(AppDbContext db, CurrentUserService current) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? q, CancellationToken ct)
    {
        var query = db.Interests.AsQueryable();
        if (!string.IsNullOrWhiteSpace(q)) query = query.Where(x => x.Name.Contains(q.Trim()));
        return Ok(await query.OrderBy(x => x.Name).Take(50).Select(x => new { x.Id, x.Name }).ToListAsync(ct));
    }

    [HttpPut("me")]
    public async Task<IActionResult> SetMine([FromBody] List<string> names, CancellationToken ct)
    {
        if (names.Count > 20) return BadRequest(new { message = "Maximum 20 interests." });
        var p = await current.GetProfileAsync(ct); if (p == null) return NotFound();
        var normalized = names.Select(x => x.Trim()).Where(x => x.Length is > 0 and <= 60)
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var interests = new List<Interest>();
        foreach (var name in normalized)
        {
            var interest = await db.Interests.FirstOrDefaultAsync(x => x.Name.ToLower() == name.ToLower(), ct);
            if (interest == null) { interest = new Interest { Id = Guid.NewGuid(), Name = name }; db.Interests.Add(interest); }
            interests.Add(interest);
        }
        p.Interests.Clear();
        foreach (var interest in interests) p.Interests.Add(interest);
        await db.SaveChangesAsync(ct);
        return Ok(interests.Select(x => x.Name).OrderBy(x => x));
    }
}
