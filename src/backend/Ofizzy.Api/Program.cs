using Ofizzy.Api.Modules.Tenancy;
using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Ofizzy.Api.Authentication;
using Ofizzy.Api.Infrastructure.Errors;
using Ofizzy.Api.Infrastructure.Persistence;

QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddJsonConsole(options => options.IncludeScopes = true);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);

    if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("Postgres")) ||
        string.IsNullOrWhiteSpace(builder.Configuration["Jwt:SigningKey"]))
    {
        for (var dir = new DirectoryInfo(Directory.GetCurrentDirectory()); dir != null; dir = dir.Parent)
        {
            var envPath = Path.Combine(dir.FullName, ".env");
            if (File.Exists(envPath))
            {
                var dict = new Dictionary<string, string?>();
                foreach (var line in File.ReadAllLines(envPath))
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith('#')) continue;
                    var parts = trimmed.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim();
                        var val = parts[1].Trim().Trim('"').Trim('\'');
                        if (key == "JWT_SIGNING_KEY" && string.IsNullOrWhiteSpace(builder.Configuration["Jwt:SigningKey"]))
                            dict["Jwt:SigningKey"] = val;
                        else if (key == "POSTGRES_PASSWORD" && string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("Postgres")))
                            dict["ConnectionStrings:Postgres"] = $"Host=localhost;Port=5432;Database=ofizzy;Username=ofizzy;Password={val}";
                    }
                }
                if (dict.Count > 0)
                    builder.Configuration.AddInMemoryCollection(dict);
                break;
            }
        }
    }

    var developmentDefaults = new Dictionary<string, string?>();
    if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("Postgres")))
        developmentDefaults["ConnectionStrings:Postgres"] = "Host=localhost;Port=5432;Database=ofizzy;Username=ofizzy";
    if (string.IsNullOrWhiteSpace(builder.Configuration["Jwt:SigningKey"]))
        developmentDefaults["Jwt:SigningKey"] = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(48));
    if (developmentDefaults.Count > 0)
        builder.Configuration.AddInMemoryCollection(developmentDefaults);
}

var connectionString = builder.Configuration.GetConnectionString("Postgres");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("ConnectionStrings:Postgres is required.");

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new();
if (Encoding.UTF8.GetByteCount(jwt.SigningKey) < 32)
    throw new InvalidOperationException("Jwt:SigningKey must contain at least 32 bytes.");

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<CurrentTenant>();
builder.Services.AddScoped<TenantProvisioningService>();
builder.Services.AddScoped<TenantAccessFilter>();
builder.Services.AddScoped<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, PlatformAuthorizationHandler>();
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<User>, Microsoft.AspNetCore.Identity.PasswordHasher<User>>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? jwt;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidIssuer = jwtOptions.Issuer,
        ValidateAudience = true, ValidAudience = jwtOptions.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
        ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30)
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies[SessionService.AccessCookie];
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddAuthorization(options => options.AddPolicy("PlatformAdmin", policy => policy.RequireAuthenticatedUser().AddRequirements(new PlatformAdminRequirement())));
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = "ofizzy_xsrf_protection";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});
builder.Services.AddControllersWithViews(options => { options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()); options.Filters.AddService<TenantAccessFilter>(); }).AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddHealthChecks().AddNpgSql(connectionString, name: "postgres", tags: ["ready"]);
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("auth", limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
});

var app = builder.Build();
if (args.Contains("--grant-platform-admin", StringComparer.OrdinalIgnoreCase))
{
    var index = Array.FindIndex(args, x => x == "--grant-platform-admin");
    if (index + 1 >= args.Length || !Guid.TryParse(args[index + 1], out var id))
        throw new InvalidOperationException("Provide the existing operator user UUID.");
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var user = await db.Users.SingleAsync(x => x.Id == id && x.IsActive);
    user.IsPlatformAdmin = true; user.PlatformAdminGrantedAt = DateTimeOffset.UtcNow;
    await db.SaveChangesAsync();
    app.Logger.LogInformation("Platform administrator explicitly granted by server operator to {UserId}", id);
    return;
}
if (args.Contains("--migrate", StringComparer.OrdinalIgnoreCase))
{
    await ReconcileMigrationHistoryAsync(connectionString);
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
    await ReconcileMigrationHistoryAsync(connectionString);
    return;
}
if (app.Environment.IsDevelopment())
{
    await ReconcileMigrationHistoryAsync(connectionString);
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
    await ReconcileMigrationHistoryAsync(connectionString);
}
app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseRateLimiter();
app.UseAuthentication();
app.UseMiddleware<TenantContextMiddleware>();
app.UseAuthorization();
app.Use(async (context, next) =>
{
    if (HttpMethods.IsGet(context.Request.Method) && !context.Request.Path.StartsWithSegments("/health"))
    {
        var antiforgery = context.RequestServices.GetRequiredService<Microsoft.AspNetCore.Antiforgery.IAntiforgery>();
        var tokens = antiforgery.GetAndStoreTokens(context);
        if (tokens.RequestToken is not null)
            context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken, new CookieOptions { HttpOnly = false, Secure = !app.Environment.IsDevelopment(), SameSite = SameSiteMode.Strict, Path = "/" });
    }
    await next();
});
if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.MapHealthChecks("/health/live", new() { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new() { Predicate = check => check.Tags.Contains("ready") });
app.MapControllers();
app.Run();

static async Task ReconcileMigrationHistoryAsync(string connectionString)
{
    await using var connection = new Npgsql.NpgsqlConnection(connectionString);
    await connection.OpenAsync();
    await using var command = connection.CreateCommand();
    command.CommandText =
        """
        DO $ofizzy$
        BEGIN
            IF EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = 'ofizzy') THEN
                CREATE TABLE IF NOT EXISTS public."__EFMigrationsHistory" (
                    "MigrationId" character varying(150) NOT NULL PRIMARY KEY,
                    "ProductVersion" character varying(32) NOT NULL
                );
                CREATE TABLE IF NOT EXISTS ofizzy."__EFMigrationsHistory" (
                    "MigrationId" character varying(150) NOT NULL PRIMARY KEY,
                    "ProductVersion" character varying(32) NOT NULL
                );
                INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                SELECT "MigrationId", "ProductVersion"
                FROM ofizzy."__EFMigrationsHistory"
                ON CONFLICT ("MigrationId") DO NOTHING;
                INSERT INTO ofizzy."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
                SELECT "MigrationId", "ProductVersion"
                FROM public."__EFMigrationsHistory"
                ON CONFLICT ("MigrationId") DO NOTHING;
            END IF;
        END
        $ofizzy$;
        """;
    await command.ExecuteNonQueryAsync();
}

public partial class Program;
