using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MatchApp.Backend.Services;

public class JwtService(IConfiguration configuration)
{
    public string CreateToken(string userId, string email, Guid profileId)
    {
        var key = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key is missing.");
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var minutes = configuration.GetValue("Jwt:ExpiresMinutes", 10080);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim("profile_id", profileId.ToString())
        };
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(issuer, audience, claims,
            expires: DateTime.UtcNow.AddMinutes(minutes), signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
