using Microsoft.EntityFrameworkCore;
using SportPneus.Api.Modules.Customers;
using SportPneus.Api.Modules.Parts;
using SportPneus.Api.Modules.Services;
using SportPneus.Api.Modules.Vehicles;
using SportPneus.Api.Modules.WorkOrders;

namespace SportPneus.Api.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<ServiceItem> Services => Set<ServiceItem>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WorkOrderService> WorkOrderServices => Set<WorkOrderService>();
    public DbSet<WorkOrderPart> WorkOrderParts => Set<WorkOrderPart>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("sport_pneus");
        modelBuilder.HasSequence<long>("work_order_number_seq");
        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("companies"); entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.LegalName).HasMaxLength(160);
            entity.Property(x => x.Cnpj).HasMaxLength(18); entity.Property(x => x.Phone).HasMaxLength(20);
            entity.Property(x => x.WhatsApp).HasMaxLength(20); entity.Property(x => x.Email).HasMaxLength(254);
            entity.Property(x => x.Address).HasMaxLength(500);
            entity.Property(x => x.City).HasMaxLength(100); entity.Property(x => x.State).HasMaxLength(2);
            entity.Property(x => x.PostalCode).HasMaxLength(10);
            entity.Property(x => x.LogoPath).HasMaxLength(300);
            entity.Property(x => x.WarrantyTerms).HasMaxLength(1000);
            entity.Property(x => x.ReceiptNotes).HasMaxLength(1000);
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
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers"); entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired(); entity.Property(x => x.Document).HasMaxLength(14);
            entity.Property(x => x.Phone).HasMaxLength(20); entity.Property(x => x.WhatsApp).HasMaxLength(20); entity.Property(x => x.Email).HasMaxLength(254);
            entity.Property(x => x.Address).HasMaxLength(500); entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.HasIndex(x => x.Name); entity.HasIndex(x => x.Phone); entity.HasIndex(x => x.IsActive);
            entity.HasIndex(x => x.Document).IsUnique().HasFilter("\"Document\" IS NOT NULL");
        });
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.ToTable("vehicles"); entity.HasKey(x => x.Id); entity.Property(x => x.Plate).HasMaxLength(8).IsRequired();
            entity.Property(x => x.Brand).HasMaxLength(80); entity.Property(x => x.Model).HasMaxLength(120).IsRequired(); entity.Property(x => x.Color).HasMaxLength(50);
            entity.Property(x => x.Chassis).HasMaxLength(40); entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.HasIndex(x => x.Plate).IsUnique(); entity.HasIndex(x => x.Model); entity.HasIndex(x => x.IsActive); entity.HasIndex(x => x.CustomerId);
            entity.HasOne(x => x.Customer).WithMany(x => x.Vehicles).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<ServiceItem>(entity =>
        {
            entity.ToTable("services"); entity.HasKey(x => x.Id); entity.Property(x => x.Name).HasMaxLength(160).IsRequired(); entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.DefaultPrice).HasPrecision(14, 2); entity.HasIndex(x => x.Name);
        });
        modelBuilder.Entity<Part>(entity =>
        {
            entity.ToTable("parts"); entity.HasKey(x => x.Id); entity.Property(x => x.Name).HasMaxLength(160).IsRequired(); entity.Property(x => x.Code).HasMaxLength(80).IsRequired();
            entity.Property(x => x.CostPrice).HasPrecision(14, 2); entity.Property(x => x.SalePrice).HasPrecision(14, 2); entity.HasIndex(x => x.Name); entity.HasIndex(x => x.Code).IsUnique();
        });
        modelBuilder.Entity<WorkOrder>(entity =>
        {
            entity.ToTable("work_orders"); entity.HasKey(x => x.Id); entity.Property(x => x.Number).HasDefaultValueSql("nextval('sport_pneus.work_order_number_seq')");
            entity.HasIndex(x => x.Number).IsUnique();
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => new { x.Status, x.CreatedAt });
            entity.HasIndex(x => x.CustomerName);
            entity.HasIndex(x => x.VehiclePlate);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20); entity.Property(x => x.CustomerName).HasMaxLength(160); entity.Property(x => x.CustomerDocument).HasMaxLength(14); entity.Property(x => x.CustomerPhone).HasMaxLength(20); entity.Property(x => x.VehiclePlate).HasMaxLength(8); entity.Property(x => x.VehicleDescription).HasMaxLength(300); entity.Property(x => x.Complaint).HasMaxLength(3000); entity.Property(x => x.Diagnosis).HasMaxLength(5000); entity.Property(x => x.Notes).HasMaxLength(3000); entity.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict); entity.HasOne(x => x.Vehicle).WithMany().HasForeignKey(x => x.VehicleId).OnDelete(DeleteBehavior.Restrict);
        });
        modelBuilder.Entity<WorkOrderService>(entity =>
        {
            entity.ToTable("work_order_services"); entity.HasKey(x => x.Id); entity.Property(x => x.Description).HasMaxLength(300); entity.Property(x => x.Quantity).HasPrecision(12, 3); entity.Property(x => x.UnitPrice).HasPrecision(14, 2); entity.HasOne(x => x.WorkOrder).WithMany(x => x.Services).HasForeignKey(x => x.WorkOrderId).OnDelete(DeleteBehavior.Cascade); entity.HasOne(x => x.Service).WithMany().HasForeignKey(x => x.ServiceId).OnDelete(DeleteBehavior.SetNull);
        });
        modelBuilder.Entity<WorkOrderPart>(entity =>
        {
            entity.ToTable("work_order_parts"); entity.HasKey(x => x.Id); entity.Property(x => x.Description).HasMaxLength(300); entity.Property(x => x.Code).HasMaxLength(80); entity.Property(x => x.Quantity).HasPrecision(12, 3); entity.Property(x => x.UnitPrice).HasPrecision(14, 2); entity.HasOne(x => x.WorkOrder).WithMany(x => x.Parts).HasForeignKey(x => x.WorkOrderId).OnDelete(DeleteBehavior.Cascade); entity.HasOne(x => x.Part).WithMany().HasForeignKey(x => x.PartId).OnDelete(DeleteBehavior.SetNull);
        });
    }
}
