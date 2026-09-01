using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SportPneus.Api.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace SportPneus.IntegrationTests;

public sealed class SportPneusFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:18-alpine").WithDatabase("sport_pneus_tests").WithUsername("tests").WithPassword("tests-password").Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Postgres"] = _postgres.GetConnectionString(),
            ["Jwt:SigningKey"] = "integration-test-signing-key-at-least-32-bytes"
        }));
        builder.ConfigureServices(services =>
        {
            services.Remove(services.Single(x => x.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)));
            services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(_postgres.GetConnectionString()));
        });
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await using var scope = Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync();
    }
}
