using Ofizzy.Api.Modules.Tenancy;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Ofizzy.Api.Infrastructure.Persistence;

namespace Ofizzy.Api.Authentication;
public sealed class SessionService(ApplicationDbContext db, IOptions<JwtOptions> options, IWebHostEnvironment environment, CurrentTenant current)
{
    public const string AccessCookie = "ofizzy_access";
    public const string RefreshCookie = "ofizzy_refresh";
    private readonly JwtOptions _options = options.Value;

    public async Task IssueAsync(User user, HttpResponse response, Guid? familyId = null, RefreshToken? replacedToken = null, CancellationToken cancellationToken = default, Guid? selectedTenantId = null, bool autoSelect = true)
    {
        var memberships = await db.TenantUsers.Include(x => x.Tenant).ThenInclude(x => x.Modules)
            .Where(x => x.UserId == user.Id && x.IsActive && (x.Tenant.Status == TenantStatus.Active || x.Tenant.Status == TenantStatus.Pending)).ToListAsync(cancellationToken);
        var selected = selectedTenantId.HasValue ? memberships.SingleOrDefault(x => x.TenantId == selectedTenantId) : autoSelect && memberships.Count == 1 ? memberships[0] : null;
        if (selectedTenantId.HasValue && selected is null) throw new InvalidOperationException("Tenant membership is no longer available.");
        current.TenantId = selected?.TenantId; current.Tenant = selected?.Tenant; current.Role = selected?.Role;
        current.UserId = user.Id; current.IsPlatformAdmin = user.IsPlatformAdmin;
        var now = DateTimeOffset.UtcNow; var accessExpires = now.AddMinutes(_options.AccessTokenMinutes);
        var claims = new List<Claim> { new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), new Claim(JwtRegisteredClaimNames.Email, user.Email), new Claim(ClaimTypes.Name, user.Name), new Claim(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()) };
        if (selected is not null) { claims.Add(new Claim("tenant", selected.TenantId.ToString())); claims.Add(new Claim("role", selected.Role.ToString())); }
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)), SecurityAlgorithms.HmacSha256);
        var jwt = new JwtSecurityToken(_options.Issuer, _options.Audience, claims, now.UtcDateTime, accessExpires.UtcDateTime, credentials);
        var plainRefresh = Convert.ToHexString(RandomNumberGenerator.GetBytes(48));
        var refresh = new RefreshToken { TenantId = selected?.TenantId, UserId = user.Id, FamilyId = familyId ?? Guid.CreateVersion7(), TokenHash = Hash(plainRefresh), ExpiresAt = now.AddDays(_options.RefreshTokenDays) };
        db.RefreshTokens.Add(refresh);
        if (replacedToken is not null) { replacedToken.RevokedAt = now; replacedToken.ReplacedByTokenId = refresh.Id; }
        await db.SaveChangesAsync(cancellationToken);
        response.Cookies.Append(AccessCookie, new JwtSecurityTokenHandler().WriteToken(jwt), Cookie(accessExpires));
        response.Cookies.Append(RefreshCookie, plainRefresh, Cookie(refresh.ExpiresAt));
    }

    public async Task<User?> RotateAsync(string plainToken, HttpResponse response, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var hash = Hash(plainToken);
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({hash}, 0))", cancellationToken);
        var token = await db.RefreshTokens.Include(x => x.User).SingleOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);
        if (token is null) return null;
        if (token.RevokedAt is not null) { await RevokeFamilyAsync(token.FamilyId, cancellationToken); await transaction.CommitAsync(cancellationToken); return null; }
        if (token.ExpiresAt <= DateTimeOffset.UtcNow || !token.User.IsActive) return null;
        if (token.TenantId.HasValue && !await db.TenantUsers.AnyAsync(x => x.UserId == token.UserId && x.TenantId == token.TenantId && x.IsActive && (x.Tenant.Status == TenantStatus.Active || x.Tenant.Status == TenantStatus.Pending), cancellationToken)) return null;
        await IssueAsync(token.User, response, token.FamilyId, token, cancellationToken, token.TenantId, autoSelect: false);
        await transaction.CommitAsync(cancellationToken); return token.User;
    }

    public async Task RevokeAsync(string? plainToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(plainToken)) return;
        var token = await db.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == Hash(plainToken), cancellationToken);
        if (token is not null && token.RevokedAt is null) { token.RevokedAt = DateTimeOffset.UtcNow; await db.SaveChangesAsync(cancellationToken); }
    }
    public void Clear(HttpResponse response)
    {
        response.Cookies.Delete(AccessCookie, Cookie(DateTimeOffset.UnixEpoch));
        response.Cookies.Delete(RefreshCookie, Cookie(DateTimeOffset.UnixEpoch));
        response.Cookies.Delete("XSRF-TOKEN", new CookieOptions { Path = "/" });
        response.Cookies.Delete("ofizzy_xsrf_protection", new CookieOptions { Path = "/" });
    }
    private async Task RevokeFamilyAsync(Guid familyId, CancellationToken cancellationToken)
    {
        var active = await db.RefreshTokens.Where(x => x.FamilyId == familyId && x.RevokedAt == null).ToListAsync(cancellationToken);
        foreach (var token in active) token.RevokedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }
    private CookieOptions Cookie(DateTimeOffset expires) => new() { HttpOnly = true, Secure = !environment.IsDevelopment(), SameSite = SameSiteMode.Strict, Path = "/", Expires = expires, IsEssential = true };
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
