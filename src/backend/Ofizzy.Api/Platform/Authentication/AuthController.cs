using Ofizzy.Api.Infrastructure.Persistence;
using Ofizzy.Api.Modules.Tenancy;
using Ofizzy.Api.Shared.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace Ofizzy.Api.Authentication;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    ApplicationDbContext db,
    IValidator<LoginRequest> validator,
    IPasswordHasher<User> passwordHasher,
    SessionService sessions,
    CurrentTenant current) : ControllerBase
{
    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<CurrentUserResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));
        }

        var normalized = request.Email.Trim().ToUpperInvariant();
        var user = await db.Users.SingleOrDefaultAsync(x => x.NormalizedEmail == normalized, cancellationToken);

        if (user is null || !user.IsActive || passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            return Problem(
                statusCode: 401,
                title: "Credenciais inválidas",
                detail: "E-mail ou senha inválidos.");
        }

        await sessions.IssueAsync(user, Response, cancellationToken: cancellationToken);
        return Ok(ResponseFor(user));
    }

    [HttpPost("refresh")]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<CurrentUserResponse>> Refresh(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(SessionService.RefreshCookie, out var token))
        {
            return Unauthorized();
        }

        var user = await sessions.RotateAsync(token, Response, cancellationToken);
        if (user is null)
        {
            sessions.Clear(Response);
            return Unauthorized();
        }

        return Ok(ResponseFor(user));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        Request.Cookies.TryGetValue(SessionService.RefreshCookie, out var token);
        await sessions.RevokeAsync(token, cancellationToken);
        sessions.Clear(Response);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserResponse>> Me(CancellationToken cancellationToken)
    {
        var user = await db.Users.SingleOrDefaultAsync(x => x.Id == current.UserId && x.IsActive, cancellationToken);
        return user is null ? Unauthorized() : Ok(ResponseFor(user));
    }

    [Authorize]
    [HttpGet("tenants")]
    public async Task<ActionResult<IReadOnlyList<TenantContextResponse>>> Tenants(CancellationToken ct)
    {
        var list = await db.TenantUsers.AsNoTracking()
            .Where(x => x.UserId == current.UserId && x.IsActive && (x.Tenant.Status == TenantStatus.Active || x.Tenant.Status == TenantStatus.Pending))
            .Select(x => new TenantContextResponse(
                x.TenantId,
                x.Tenant.Name,
                x.Tenant.Slug,
                x.Tenant.Status.ToString(),
                x.Tenant.Vertical.ToString(),
                x.Role.ToString(),
                x.Tenant.OnboardingCompletedAt != null,
                x.Tenant.Modules.Where(m => m.Enabled).Select(m => m.Module.ToString()).ToArray()))
            .ToListAsync(ct);

        return Ok(list);
    }

    [Authorize]
    [HttpPost("tenant")]
    public async Task<ActionResult<CurrentUserResponse>> SelectTenant(
        SelectTenantRequest request,
        CancellationToken ct)
    {
        var hasAccess = await db.TenantUsers.AnyAsync(
            x => x.TenantId == request.TenantId
                && x.UserId == current.UserId
                && x.IsActive
                && (x.Tenant.Status == TenantStatus.Active || x.Tenant.Status == TenantStatus.Pending),
            ct);

        if (!hasAccess)
        {
            return Problem(statusCode: 403, title: "Organização indisponível");
        }

        var user = await db.Users.SingleAsync(x => x.Id == current.UserId, ct);
        Request.Cookies.TryGetValue(SessionService.RefreshCookie, out var refresh);
        await sessions.RevokeAsync(refresh, ct);
        await sessions.IssueAsync(user, Response, cancellationToken: ct, selectedTenantId: request.TenantId);

        return Ok(ResponseFor(user));
    }

    private CurrentUserResponse ResponseFor(User user)
    {
        var tenantContext = current.Tenant is { } tenant
            ? new TenantContextResponse(
                tenant.Id,
                tenant.Name,
                tenant.Slug,
                tenant.Status.ToString(),
                tenant.Vertical.ToString(),
                current.Role.ToString()!,
                tenant.OnboardingCompletedAt != null,
                tenant.Modules.Where(x => x.Enabled).Select(x => x.Module.ToString()).ToArray())
            : null;

        return new CurrentUserResponse(
            user.Id,
            user.Name,
            user.Email,
            user.IsPlatformAdmin,
            tenantContext);
    }
}
