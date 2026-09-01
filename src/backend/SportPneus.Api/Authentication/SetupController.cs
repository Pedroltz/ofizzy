using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using SportPneus.Api.Infrastructure.Errors;
using SportPneus.Api.Infrastructure.Persistence;
using SportPneus.Api.Shared.Validation;

namespace SportPneus.Api.Authentication;
[ApiController, Route("api/setup")]
public sealed class SetupController(ApplicationDbContext db, IValidator<SetupRequest> validator, IPasswordHasher<User> passwordHasher, SessionService sessions) : ControllerBase
{
    [HttpGet("status")]
    public async Task<ActionResult<SetupStatusResponse>> Status(CancellationToken cancellationToken) => Ok(new SetupStatusResponse(!await db.Users.AnyAsync(cancellationToken)));

    [HttpPost, EnableRateLimiting("auth")]
    public async Task<ActionResult<CurrentUserResponse>> Create(SetupRequest request, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return ValidationProblem(new ValidationProblemDetails(validation.ToDictionary()));
        await using var transaction = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
        if (await db.Users.AnyAsync(cancellationToken)) throw new ConflictException("A configuração inicial já foi concluída.");
        var user = new User { Name = request.AdminName.Trim(), Email = request.Email.Trim().ToLowerInvariant(), NormalizedEmail = request.Email.Trim().ToUpperInvariant() };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        db.Companies.Add(new Company { Name = request.CompanyName.Trim(), Cnpj = Digits(request.Cnpj), Phone = request.Phone?.Trim() }); db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken); await sessions.IssueAsync(user, Response, cancellationToken: cancellationToken); await transaction.CommitAsync(cancellationToken);
        return Created("/api/auth/me", new CurrentUserResponse(user.Id, user.Name, user.Email));
    }
    private static string? Digits(string? value) => string.IsNullOrWhiteSpace(value) ? null : new string(value.Where(char.IsDigit).ToArray());
}
