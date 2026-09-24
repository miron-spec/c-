using MatchApp.Backend.Data;
using MatchApp.Backend.DTOs;
using MatchApp.Backend.Entities;
using MatchApp.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MatchApp.Backend.Controllers;

[ApiController, Route("api/auth")]
public class AuthController(AppDbContext db, PasswordService passwords, JwtService jwt) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || request.Password.Length < 8)
            return BadRequest(new { message = "Email is required and password must be at least 8 characters." });
        if (await db.UserAccounts.AnyAsync(x => x.Email == email, ct))
            return Conflict(new { message = "Email already exists." });

        var profile = new UserProfile
        {
            Id = Guid.NewGuid(), UserId = Guid.NewGuid().ToString(), Name = "Новий користувач", Age = 18,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };
        db.UserProfiles.Add(profile);
        db.UserAccounts.Add(new UserAccount
        {
            Id = Guid.NewGuid(), Email = email, PasswordHash = passwords.Hash(request.Password),
            ProfileId = profile.Id, CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
        var token = jwt.CreateToken(profile.UserId, email, profile.Id);
        return Ok(new AuthResponse(profile.UserId, token, profile.Id));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var account = await db.UserAccounts.Include(x => x.Profile)
            .FirstOrDefaultAsync(x => x.Email == email, ct);
        if (account == null || !passwords.Verify(account.PasswordHash, request.Password))
            return Unauthorized(new { message = "Invalid credentials." });

        var token = jwt.CreateToken(account.Profile.UserId, email, account.Profile.Id);
        return Ok(new AuthResponse(account.Profile.UserId, token, account.Profile.Id));
    }

    [Authorize]
    [HttpGet("me")]
    public IActionResult Me() => Ok(new { userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value });
}
