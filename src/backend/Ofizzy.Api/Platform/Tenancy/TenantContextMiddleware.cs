using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Ofizzy.Api.Infrastructure.Persistence;

namespace Ofizzy.Api.Modules.Tenancy;

public sealed class TenantContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext http,
        ApplicationDbContext db,
        CurrentTenant current,
        ILogger<TenantContextMiddleware> logger)
    {
        if (http.User.Identity?.IsAuthenticated == true)
        {
            if (!Guid.TryParse(http.User.FindFirstValue("sub"), out var userId))
            {
                http.Response.StatusCode = 401;
                return;
            }

            var user = await db.Users
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == userId && x.IsActive, http.RequestAborted);

            if (user is null)
            {
                http.Response.StatusCode = 401;
                return;
            }

            current.UserId = user.Id;
            current.IsPlatformAdmin = user.IsPlatformAdmin;

            if (Guid.TryParse(http.User.FindFirstValue("tenant"), out var tenantId))
            {
                var membership = await db.TenantUsers
                    .AsNoTracking()
                    .Include(x => x.Tenant)
                    .ThenInclude(x => x.Modules)
                    .SingleOrDefaultAsync(x => x.UserId == userId && x.TenantId == tenantId && x.IsActive, http.RequestAborted);

                if (membership?.Tenant.Status is TenantStatus.Active or TenantStatus.Pending)
                {
                    current.TenantId = tenantId;
                    current.Tenant = membership.Tenant;
                    current.Role = membership.Role;
                }
            }
        }

        http.Items["TenantId"] = current.TenantId;
        http.Items["UserId"] = current.UserId;

        using var scope = logger.BeginScope(new Dictionary<string, object?>
        {
            ["TenantId"] = current.TenantId,
            ["UserId"] = current.UserId,
            ["RequestId"] = http.TraceIdentifier
        });

        await next(http);

        if (current.UserId.HasValue)
        {
            logger.LogInformation("Authenticated request completed with status {StatusCode}", http.Response.StatusCode);
        }
    }
}
