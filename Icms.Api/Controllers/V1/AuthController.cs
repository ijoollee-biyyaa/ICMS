using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

using Icms.Application.DTOs;
using Icms.Domain.Entities;
using Icms.Infrastructure.Identity;
using Icms.Infrastructure.Persistence;
using Icms.Infrastructure.Services;

namespace Icms.Api.Controllers.V1;

[ApiController]
[Route("api/auth")]
[Tags("Auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private const string RefreshTokenCookieName = "icms_refresh";

    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IcmsDbContext _context;
    private readonly TokenService _tokenService;

    public AuthController(
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        IcmsDbContext context,
        TokenService tokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _tokenService = tokenService;
    }

    private void SetRefreshTokenCookie(string token)
    {
        Response.Cookies.Append(RefreshTokenCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }

    private void ClearRefreshTokenCookie()
    {
        Response.Cookies.Append(RefreshTokenCookieName, string.Empty, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddDays(-1)
        });
    }

    private string? GetRefreshTokenFromCookieOrBody(string? bodyToken) =>
        Request.Cookies[RefreshTokenCookieName] ?? (string.IsNullOrEmpty(bodyToken) ? null : bodyToken);

    [HttpPost("register")]
    [IgnoreAntiforgeryToken]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [EndpointSummary("Create an account")]
    [EndpointDescription(
        "Creates a real Identity account. Passwords are hashed by UserManager (PBKDF2 with per-user salt). " +
        "Returns a generic response for duplicate emails to prevent account enumeration. " +
        "Anonymous bootstrap endpoint (exempt from antiforgery).")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return Ok(new { message = "Registration request received." });
        }

        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            FatherName = request.FatherName,
            GrandfatherName = request.GrandfatherName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { errors });
        }

        if (!await _roleManager.RoleExistsAsync(request.Role))
        {
            await _roleManager.CreateAsync(new IdentityRole(request.Role));
        }

        await _userManager.AddToRoleAsync(user, request.Role);

        return Ok(new { message = "Registration successful." });
    }

    [HttpPost("login")]
    [EnableRateLimiting("AuthLimiter")]
    [IgnoreAntiforgeryToken]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    [EndpointSummary("Sign in")]
    [EndpointDescription(
        "Validates credentials through UserManager. Failed attempts are counted; after 5 failures the account " +
        "locks for 15 minutes (423). Both unknown emails and wrong passwords return the same 401 to avoid enumeration. " +
        "On success issues a signed JWT access token (15 min) and stores a single-use refresh token (7 days) in an " +
        "HttpOnly, Secure, SameSite=Lax cookie so it survives page reloads and stays invisible to JavaScript. " +
        "Anonymous bootstrap endpoint (exempt from antiforgery).")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Username);
        if (user == null)
        {
            return Unauthorized(new { detail = "Invalid credentials." });
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return StatusCode(StatusCodes.Status423Locked, new { detail = "Account locked due to multiple failed login attempts. Try again in 15 minutes." });
        }

        if (user.IsAccountLocked)
        {
            return StatusCode(StatusCodes.Status423Locked, new { detail = "This account is locked by the administrator." });
        }

        var validPassword = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!validPassword)
        {
            await _userManager.AccessFailedAsync(user);
            return Unauthorized(new { detail = "Invalid credentials." });
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = await _tokenService.GenerateJwtAsync(user, roles);

        var refreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString("N"),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        SetRefreshTokenCookie(refreshToken.Token);

        return Ok(new { accessToken });
    }

    public record RefreshRequest(string? RefreshToken);

    public record LogoutRequest(string? RefreshToken);

    public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

    [HttpPost("refresh")]
    [IgnoreAntiforgeryToken]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Rotate refresh token")]
    [EndpointDescription(
        "Consumes the refresh token presented in the icms_refresh cookie (single use) and issues a brand-new " +
        "access token with a rotated refresh cookie. Submitting an already-used token is treated as theft: every " +
        "active token for that user is revoked. Token-lifecycle endpoint (exempt from antiforgery; the refresh " +
        "token itself is the credential).")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest? request = null)
    {
        var presentedToken = GetRefreshTokenFromCookieOrBody(request?.RefreshToken);

        if (string.IsNullOrEmpty(presentedToken))
        {
            return Unauthorized(new { detail = "Invalid refresh token." });
        }

        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == presentedToken);

        if (storedToken == null)
        {
            ClearRefreshTokenCookie();
            return Unauthorized(new { detail = "Invalid refresh token." });
        }

        if (storedToken.IsUsed)
        {
            var userTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == storedToken.UserId)
                .ToListAsync();

            foreach (var t in userTokens)
            {
                t.IsRevoked = true;
            }

            await _context.SaveChangesAsync();

            ClearRefreshTokenCookie();
            return Unauthorized(new { detail = "Token theft detected. All user sessions revoked." });
        }

        if (storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow)
        {
            ClearRefreshTokenCookie();
            return Unauthorized(new { detail = "Refresh token expired or revoked." });
        }

        storedToken.IsUsed = true;

        var newRefreshToken = new RefreshToken
        {
            Token = Guid.NewGuid().ToString("N"),
            UserId = storedToken.UserId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        var user = await _userManager.FindByIdAsync(storedToken.UserId);

        if (user == null || user.IsAccountLocked)
        {
            var lockedTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == storedToken.UserId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var t in lockedTokens)
            {
                t.IsRevoked = true;
            }

            await _context.SaveChangesAsync();

            ClearRefreshTokenCookie();
            return Unauthorized(new { detail = "Account is locked or no longer available." });
        }

        var roles = await _userManager.GetRolesAsync(user);
        var newAccessToken = await _tokenService.GenerateJwtAsync(user!, roles);

        SetRefreshTokenCookie(newRefreshToken.Token);

        return Ok(new { accessToken = newAccessToken });
    }

    [HttpPost("logout")]
    [IgnoreAntiforgeryToken]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [EndpointSummary("Sign out")]
    [EndpointDescription(
        "Revokes the refresh token presented in the icms_refresh cookie so it can no longer be rotated, then " +
        "clears the cookie. The client discards its in-memory access token. " +
        "No authentication required (the refresh token itself is the credential).")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest? request = null)
    {
        var presentedToken = GetRefreshTokenFromCookieOrBody(request?.RefreshToken);

        if (!string.IsNullOrEmpty(presentedToken))
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == presentedToken);

            if (storedToken != null)
            {
                storedToken.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
        }

        ClearRefreshTokenCookie();

        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Current user")]
    [EndpointDescription(
        "Builds the profile from the claims of the presented Bearer access token.")]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized(new { detail = "Missing identity claim in token." });
        }

        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? string.Empty;
        var firstName = User.FindFirst("FirstName")?.Value ?? string.Empty;
        var fatherName = User.FindFirst("FatherName")?.Value ?? string.Empty;
        var grandfatherName = User.FindFirst("GrandfatherName")?.Value ?? string.Empty;
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;
        var displayName = $"{firstName} {fatherName} {grandfatherName}".Trim();

        long? churchId = null;
        var churchClaim = User.FindFirst("ChurchId")?.Value;
        if (long.TryParse(churchClaim, out var cid))
        {
            churchId = cid;
        }

        long? memberId = null;
        var memberClaim = User.FindFirst("MemberId")?.Value;
        if (long.TryParse(memberClaim, out var mid))
        {
            memberId = mid;
        }

        return Ok(new UserProfileDto(userId, email, displayName, role, churchId, memberId));
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [EndpointSummary("Change own password")]
    [EndpointDescription(
        "Lets the signed-in user replace their password (used to rotate the system-generated temporary password). " +
        "Requires the current password. All existing refresh tokens are revoked so other sessions must re-login.")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized(new { detail = "Missing identity claim in token." });
        }

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { errors });
        }

        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && !rt.IsRevoked)
            .ToListAsync(ct);

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }

        await _context.SaveChangesAsync(ct);

        ClearRefreshTokenCookie();

        return NoContent();
    }
}