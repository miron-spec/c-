using MatchApp.Backend.Data;
using MatchApp.Backend.DTOs;
using MatchApp.Backend.Entities;
using MatchApp.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchApp.Backend.Controllers;

[Authorize, ApiController, Route("api/reports")]
public class ReportsController(AppDbContext db, CurrentUserService current) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(CreateReportRequest request, CancellationToken ct)
    {
        var me = await current.GetProfileAsync(ct); if (me == null) return NotFound();
        if (request.ReportedProfileId == me.Id) return BadRequest(new { message = "Cannot report yourself." });
        if (!await db.UserProfiles.AnyAsync(x => x.Id == request.ReportedProfileId, ct)) return NotFound();
        if (string.IsNullOrWhiteSpace(request.Reason)) return BadRequest(new { message = "Reason is required." });
        db.Reports.Add(new Report { Id = Guid.NewGuid(), ReporterProfileId = me.Id,
            ReportedProfileId = request.ReportedProfileId, Reason = request.Reason.Trim(), CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync(ct); return Ok(new { message = "Report submitted." });
    }
}
