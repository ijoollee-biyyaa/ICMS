using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using Icms.Infrastructure.Identity;
using Icms.Infrastructure.Persistence;

namespace Icms.Infrastructure.Services;

public class TokenService
{
    private readonly IConfiguration _config;
    private readonly IcmsDbContext _db;

    public TokenService(IConfiguration config, IcmsDbContext db)
    {
        _config = config;
        _db = db;
    }

    public async Task<string> GenerateJwtAsync(User user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
            new Claim("FirstName", user.FirstName),
            new Claim("FatherName", user.FatherName),
            new Claim("GrandfatherName", user.GrandfatherName)
        };

        var churchId = await _db.Employees.AsNoTracking()
            .Where(e => e.UserId == user.Id && e.ChurchId != null && !e.IsDeleted)
            .Select(e => e.ChurchId)
            .FirstOrDefaultAsync();

        if (churchId is { } cid)
        {
            claims.Add(new Claim("ChurchId", cid.ToString()));
        }

        var memberId = await _db.Users.AsNoTracking()
            .Where(u => u.Id == user.Id)
            .Select(u => u.MemberId)
            .FirstOrDefaultAsync();

        if (memberId is null)
        {
            // Back-compat: accounts tied to a member via employment.
            memberId = await _db.Employees.AsNoTracking()
                .Where(e => e.UserId == user.Id && e.MemberId != null && !e.IsDeleted)
                .Select(e => e.MemberId)
                .FirstOrDefaultAsync();
        }

        if (memberId is { } mid)
        {
            claims.Add(new Claim("MemberId", mid.ToString()));

            if (churchId is null)
            {
                churchId = await _db.Members.AsNoTracking()
                    .Where(m => m.Id == mid && !m.IsDeleted)
                    .Select(m => (long?)m.ChurchId)
                    .FirstOrDefaultAsync();

                if (churchId is { } mCid)
                {
                    claims.Add(new Claim("ChurchId", mCid.ToString()));
                }
            }
        }

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpiryMinutes"]!)),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}