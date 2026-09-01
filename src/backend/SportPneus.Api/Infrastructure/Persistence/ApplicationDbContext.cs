using Microsoft.EntityFrameworkCore;

namespace SportPneus.Api.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("sport_pneus");
        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("companies"); entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Cnpj).HasMaxLength(14); entity.Property(x => x.Phone).HasMaxLength(20);
            entity.Property(x => x.WhatsApp).HasMaxLength(20); entity.Property(x => x.Address).HasMaxLength(500);
            entity.Property(x => x.LogoPath).HasMaxLength(300);
        });
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users"); entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired(); entity.Property(x => x.Email).HasMaxLength(254).IsRequired();
            entity.Property(x => x.NormalizedEmail).HasMaxLength(254).IsRequired(); entity.Property(x => x.PasswordHash).HasMaxLength(600).IsRequired();
            entity.HasIndex(x => x.NormalizedEmail).IsUnique();
        });
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens"); entity.HasKey(x => x.Id); entity.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.TokenHash).IsUnique(); entity.HasIndex(x => new { x.UserId, x.FamilyId });
            entity.HasOne(x => x.User).WithMany(x => x.RefreshTokens).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
