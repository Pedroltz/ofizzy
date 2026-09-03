using System.Text;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SportPneus.Api.Authentication;
using SportPneus.Api.Infrastructure.Errors;
using SportPneus.Api.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("ConnectionStrings:Postgres is required.");
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
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<User>, Microsoft.AspNetCore.Identity.PasswordHasher<User>>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true, ValidIssuer = jwt.Issuer,
        ValidateAudience = true, ValidAudience = jwt.Audience,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)),
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
builder.Services.AddAuthorization();
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = "sport_xsrf_protection";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});
builder.Services.AddControllersWithViews(options => options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute())).AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
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
if (args.Contains("--migrate", StringComparer.OrdinalIgnoreCase))
{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
    return;
}
app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseRateLimiter();
app.UseAuthentication();
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

public partial class Program;
