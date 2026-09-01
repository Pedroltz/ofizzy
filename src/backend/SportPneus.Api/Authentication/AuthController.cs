using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SportPneus.Api.Infrastructure.Persistence;
using SportPneus.Api.Shared.Validation;

namespace SportPneus.Api.Authentication;
[ApiController, Route("api/auth")]
public sealed class AuthController(ApplicationDbContext db, IValidator<LoginRequest> validator, IPasswordHasher<User> passwordHasher, SessionService sessions) : ControllerBase
{
    [HttpPost("login"), EnableRateLimiting("auth")]
    public async Task<ActionResult<CurrentUserResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken); if (!validation.IsValid) return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));
        var normalized = request.Email.Trim().ToUpperInvariant(); var user = await db.Users.SingleOrDefaultAsync(x => x.NormalizedEmail == normalized, cancellationToken);
        if (user is null || !user.IsActive || passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            return Problem(statusCode: 401, title: "Credenciais inválidas", detail: "E-mail ou senha inválidos.");
        await sessions.IssueAsync(user, Response, cancellationToken: cancellationToken); return Ok(new CurrentUserResponse(user.Id, user.Name, user.Email));
    }

    [HttpPost("refresh"), EnableRateLimiting("auth")]
    public async Task<ActionResult<CurrentUserResponse>> Refresh(CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(SessionService.RefreshCookie, out var token)) return Unauthorized();
        var user = await sessions.RotateAsync(token, Response, cancellationToken);
        if (user is null) { sessions.Clear(Response); return Unauthorized(); }
        return Ok(new CurrentUserResponse(user.Id, user.Name, user.Email));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        Request.Cookies.TryGetValue(SessionService.RefreshCookie, out var token); await sessions.RevokeAsync(token, cancellationToken); sessions.Clear(Response); return NoContent();
    }

    [Authorize, HttpGet("me")]
    public async Task<ActionResult<CurrentUserResponse>> Me(CancellationToken cancellationToken)
    {
        var subject = User.FindFirstValue("sub"); if (!Guid.TryParse(subject, out var id)) return Unauthorized();
        var user = await db.Users.Where(x => x.Id == id && x.IsActive).Select(x => new CurrentUserResponse(x.Id, x.Name, x.Email)).SingleOrDefaultAsync(cancellationToken);
        return user is null ? Unauthorized() : Ok(user);
    }
}
