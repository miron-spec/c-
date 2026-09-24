using MatchApp.Backend.Data;
using MatchApp.Backend.DTOs;
using MatchApp.Backend.Entities;
using MatchApp.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchApp.Backend.Controllers;

[Authorize, ApiController, Route("api/preferences")]
public class PreferencesController(AppDbContext db, CurrentUserService current) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PreferencesDto>> Get(CancellationToken ct)
    {
        var p = await current.GetProfileAsync(ct); if (p == null) return NotFound();
        var pref = await db.SearchPreferences.FirstOrDefaultAsync(x => x.UserProfileId == p.Id, ct);
        pref ??= new SearchPreference { Id = Guid.NewGuid(), UserProfileId = p.Id };
        return Ok(new PreferencesDto(pref.MinAge, pref.MaxAge, pref.Gender, pref.City, pref.Purpose));
    }

    [HttpPut]
    public async Task<ActionResult<PreferencesDto>> Update(UpdatePreferencesRequest request, CancellationToken ct)
    {
        if (request.MinAge < 18 || request.MaxAge > 100 || request.MinAge > request.MaxAge)
            return BadRequest(new { message = "Invalid age range." });
        var p = await current.GetProfileAsync(ct); if (p == null) return NotFound();
        var pref = await db.SearchPreferences.FirstOrDefaultAsync(x => x.UserProfileId == p.Id, ct);
        if (pref == null)
        {
            pref = new SearchPreference { Id = Guid.NewGuid(), UserProfileId = p.Id };
            db.SearchPreferences.Add(pref);
        }
        pref.MinAge = request.MinAge; pref.MaxAge = request.MaxAge; pref.Gender = request.Gender;
        pref.City = request.City; pref.Purpose = request.Purpose;
        await db.SaveChangesAsync(ct);
        return Ok(new PreferencesDto(pref.MinAge, pref.MaxAge, pref.Gender, pref.City, pref.Purpose));
    }
}
