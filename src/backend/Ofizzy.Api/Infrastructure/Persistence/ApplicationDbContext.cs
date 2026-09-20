using Ofizzy.Api.Modules.Customers;
using Ofizzy.Api.Modules.Fiscal;
using Ofizzy.Api.Modules.Parts;
using Ofizzy.Api.Modules.Services;
using Ofizzy.Api.Modules.Tenancy;
using Ofizzy.Api.Modules.WorkOrders;
using Ofizzy.Api.Verticals.Automotive;
using Microsoft.EntityFrameworkCore;

namespace Ofizzy.Api.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, CurrentTenant? currentTenant = null) : DbContext(options)
{
    public Guid? TenantId => currentTenant?.TenantId;

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantUser> TenantUsers => Set<TenantUser>();
    public DbSet<TenantModule> TenantModules => Set<TenantModule>();
    public DbSet<TenantSettings> TenantSettings => Set<TenantSettings>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<ServiceItem> Services => Set<ServiceItem>();
    public DbSet<Part> Parts => Set<Part>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WorkOrderService> WorkOrderServices => Set<WorkOrderService>();
    public DbSet<WorkOrderPart> WorkOrderParts => Set<WorkOrderPart>();

    public DbSet<FiscalSettings> FiscalSettingsEntries => Set<FiscalSettings>();
    public DbSet<ProductFiscalProfile> ProductFiscalProfileEntries => Set<ProductFiscalProfile>();
    public DbSet<ServiceFiscalProfile> ServiceFiscalProfileEntries => Set<ServiceFiscalProfile>();
    public DbSet<FiscalPreparation> FiscalPreparationEntries => Set<FiscalPreparation>();
    public DbSet<FiscalSequence> FiscalSequenceEntries => Set<FiscalSequence>();
    public DbSet<FiscalDocument> FiscalDocumentEntries => Set<FiscalDocument>();
    public DbSet<FiscalEvent> FiscalEventEntries => Set<FiscalEvent>();
    public DbSet<FiscalInutilization> FiscalInutilizationEntries => Set<FiscalInutilization>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ofizzy");

        modelBuilder.Entity<Tenant>(e =>
        {
            e.ToTable("tenants");
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).HasMaxLength(160);
            e.Property(x => x.Slug).HasMaxLength(80);
            e.HasIndex(x => x.Slug).IsUnique();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Vertical).HasConversion<string>().HasMaxLength(40);
            e.HasOne(x => x.Settings)
                .WithOne()
                .HasForeignKey<TenantSettings>(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Modules)
                .WithOne()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
            e.Property(x => x.FiscalProductionReleased).HasDefaultValue(false);
        });

        modelBuilder.Entity<TenantUser>(e =>
        {
            e.ToTable("tenant_users");
            e.HasKey(x => new { x.TenantId, x.UserId });
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
            e.HasOne(x => x.Tenant)
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TenantModule>(e =>
        {
            e.ToTable("tenant_modules");
            e.HasKey(x => new { x.TenantId, x.Module });
            e.Property(x => x.Module).HasConversion<string>().HasMaxLength(40);
        });

        modelBuilder.Entity<TenantSettings>(entity =>
        {
            entity.ToTable("companies");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Timezone).HasMaxLength(100);
            entity.Property(x => x.Currency).HasMaxLength(3);
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.LegalName).HasMaxLength(160);
            entity.Property(x => x.Cnpj).HasMaxLength(18);
            entity.Property(x => x.Phone).HasMaxLength(20);
            entity.Property(x => x.WhatsApp).HasMaxLength(20);
            entity.Property(x => x.Email).HasMaxLength(254);
            entity.Property(x => x.Address).HasMaxLength(500);
            entity.Property(x => x.City).HasMaxLength(100);
            entity.Property(x => x.State).HasMaxLength(2);
            entity.Property(x => x.PostalCode).HasMaxLength(10);
            entity.Property(x => x.LogoPath).HasMaxLength(300);
            entity.Property(x => x.WarrantyTerms).HasMaxLength(1000);
            entity.Property(x => x.ReceiptNotes).HasMaxLength(1000);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(254).IsRequired();
            entity.Property(x => x.NormalizedEmail).HasMaxLength(254).IsRequired();
            entity.Property(x => x.PasswordHash).HasMaxLength(600).IsRequired();
            entity.HasIndex(x => x.NormalizedEmail).IsUnique();
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasIndex(x => new { x.UserId, x.FamilyId });
            entity.HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Document).HasMaxLength(14);
            entity.Property(x => x.Phone).HasMaxLength(20);
            entity.Property(x => x.WhatsApp).HasMaxLength(20);
            entity.Property(x => x.Email).HasMaxLength(254);
            entity.Property(x => x.Address).HasMaxLength(500);
            entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.Property(x => x.PostalCode).HasMaxLength(8);
            entity.Property(x => x.Street).HasMaxLength(60);
            entity.Property(x => x.Number).HasMaxLength(60);
            entity.Property(x => x.District).HasMaxLength(60);
            entity.Property(x => x.City).HasMaxLength(60);
            entity.Property(x => x.State).HasMaxLength(2);
            entity.Property(x => x.CityCode).HasMaxLength(7);
            entity.Property(x => x.StateRegistration).HasMaxLength(14);
            entity.HasIndex(x => x.Name);
            entity.HasIndex(x => x.Phone);
            entity.HasIndex(x => x.IsActive);
            entity.HasIndex(x => x.Document).IsUnique().HasFilter("\"Document\" IS NOT NULL");
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.ToTable("vehicles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Plate).HasMaxLength(8).IsRequired();
            entity.Property(x => x.Brand).HasMaxLength(80);
            entity.Property(x => x.Model).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Color).HasMaxLength(50);
            entity.Property(x => x.Chassis).HasMaxLength(40);
            entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.HasIndex(x => x.Plate).IsUnique();
            entity.HasIndex(x => x.Model);
            entity.HasIndex(x => x.IsActive);
            entity.HasIndex(x => x.CustomerId);
            entity.HasOne(x => x.Customer)
                .WithMany(x => x.Vehicles)
                .HasForeignKey(x => new { x.TenantId, x.CustomerId })
                .HasPrincipalKey(x => new { x.TenantId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ServiceItem>(entity =>
        {
            entity.ToTable("services");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.DefaultPrice).HasPrecision(14, 2);
            entity.HasIndex(x => x.Name);
        });

        modelBuilder.Entity<Part>(entity =>
        {
            entity.ToTable("parts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(160).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(80).IsRequired();
            entity.Property(x => x.CostPrice).HasPrecision(14, 2);
            entity.Property(x => x.SalePrice).HasPrecision(14, 2);
            entity.HasIndex(x => x.Name);
            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<WorkOrder>(entity =>
        {
            entity.ToTable("work_orders");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Number).ValueGeneratedNever();
            entity.HasIndex(x => x.Number).IsUnique();
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => new { x.Status, x.CreatedAt });
            entity.HasIndex(x => x.CustomerName);
            entity.HasIndex(x => x.VehiclePlate);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.CustomerName).HasMaxLength(160);
            entity.Property(x => x.CustomerDocument).HasMaxLength(14);
            entity.Property(x => x.CustomerPhone).HasMaxLength(20);
            entity.Property(x => x.VehiclePlate).HasMaxLength(8);
            entity.Property(x => x.VehicleDescription).HasMaxLength(300);
            entity.Property(x => x.Complaint).HasMaxLength(3000);
            entity.Property(x => x.Diagnosis).HasMaxLength(5000);
            entity.Property(x => x.Notes).HasMaxLength(3000);
            entity.HasOne(x => x.Customer)
                .WithMany()
                .HasForeignKey(x => new { x.TenantId, x.CustomerId })
                .HasPrincipalKey(x => new { x.TenantId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Vehicle)
                .WithMany()
                .HasForeignKey(x => new { x.TenantId, x.VehicleId })
                .HasPrincipalKey(x => new { x.TenantId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WorkOrderService>(entity =>
        {
            entity.ToTable("work_order_services");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Description).HasMaxLength(300);
            entity.Property(x => x.Quantity).HasPrecision(12, 3);
            entity.Property(x => x.UnitPrice).HasPrecision(14, 2);
            entity.HasOne(x => x.WorkOrder)
                .WithMany(x => x.Services)
                .HasForeignKey(x => new { x.TenantId, x.WorkOrderId })
                .HasPrincipalKey(x => new { x.TenantId, x.Id })
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Service)
                .WithMany()
                .HasForeignKey(x => new { x.TenantId, x.ServiceId })
                .HasPrincipalKey(x => new { x.TenantId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WorkOrderPart>(entity =>
        {
            entity.ToTable("work_order_parts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Description).HasMaxLength(300);
            entity.Property(x => x.Code).HasMaxLength(80);
            entity.Property(x => x.Quantity).HasPrecision(12, 3);
            entity.Property(x => x.UnitPrice).HasPrecision(14, 2);
            entity.HasOne(x => x.WorkOrder)
                .WithMany(x => x.Parts)
                .HasForeignKey(x => new { x.TenantId, x.WorkOrderId })
                .HasPrincipalKey(x => new { x.TenantId, x.Id })
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Part)
                .WithMany()
                .HasForeignKey(x => new { x.TenantId, x.PartId })
                .HasPrincipalKey(x => new { x.TenantId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
        });

        Scope<TenantSettings>(modelBuilder);
        Scope<Customer>(modelBuilder);
        Scope<Vehicle>(modelBuilder);
        Scope<ServiceItem>(modelBuilder);
        Scope<Part>(modelBuilder);
        Scope<WorkOrder>(modelBuilder);
        Scope<WorkOrderService>(modelBuilder);
        Scope<WorkOrderPart>(modelBuilder);

        modelBuilder.Entity<FiscalSettings>(e =>
        {
            e.ToTable("fiscal_settings");
            e.HasIndex(x => x.TenantId).IsUnique();
            e.Property(x => x.Version).IsConcurrencyToken();
        });

        modelBuilder.Entity<ProductFiscalProfile>(e =>
        {
            e.ToTable("fiscal_products");
            e.HasIndex(x => x.PartId).IsUnique();
            e.HasOne<Part>()
                .WithMany()
                .HasForeignKey(x => new { x.TenantId, x.PartId })
                .HasPrincipalKey(x => new { x.TenantId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ServiceFiscalProfile>(e =>
        {
            e.ToTable("fiscal_services");
            e.HasIndex(x => x.ServiceId).IsUnique();
            e.HasOne<ServiceItem>()
                .WithMany()
                .HasForeignKey(x => new { x.TenantId, x.ServiceId })
                .HasPrincipalKey(x => new { x.TenantId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FiscalPreparation>(e =>
        {
            e.ToTable("fiscal_preparations");
            e.HasIndex(x => x.WorkOrderId).IsUnique();
            e.Property(x => x.Version).IsConcurrencyToken();
            e.HasOne<WorkOrder>()
                .WithMany()
                .HasForeignKey(x => new { x.TenantId, x.WorkOrderId })
                .HasPrincipalKey(x => new { x.TenantId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FiscalSequence>(e =>
        {
            e.ToTable("fiscal_sequences");
            e.HasIndex(x => new { x.Kind, x.Environment, x.Series }).IsUnique();
            e.Property(x => x.Version).IsConcurrencyToken();
        });

        modelBuilder.Entity<FiscalDocument>(e =>
        {
            e.ToTable("fiscal_documents");
            e.Property(x => x.Version).IsConcurrencyToken();
            e.Property(x => x.Total).HasPrecision(18, 2);
            e.Property(x => x.SchemaPackage).HasMaxLength(80).IsRequired();
            e.HasAlternateKey(x => new { x.TenantId, x.Id });
            e.HasIndex(x => new { x.Kind, x.Environment, x.Series, x.Number }).IsUnique();
            e.HasIndex(x => new { x.WorkOrderId, x.Kind, x.Environment })
                .IsUnique()
                .HasFilter("\"State\" NOT IN (6, 7)");
            e.HasOne<WorkOrder>()
                .WithMany()
                .HasForeignKey(x => new { x.TenantId, x.WorkOrderId })
                .HasPrincipalKey(x => new { x.TenantId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FiscalEvent>(e =>
        {
            e.ToTable("fiscal_events");
            e.HasOne<FiscalDocument>()
                .WithMany()
                .HasForeignKey(x => new { x.TenantId, x.DocumentId })
                .HasPrincipalKey(x => new { x.TenantId, x.Id })
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<FiscalInutilization>(e =>
        {
            e.ToTable("fiscal_inutilizations");
            e.Property(x => x.Version).IsConcurrencyToken();
            e.HasIndex(x => new { x.Environment, x.Series, x.Year, x.FirstNumber, x.LastNumber }).IsUnique();
        });

        Scope<FiscalInutilization>(modelBuilder);
        Scope<FiscalSettings>(modelBuilder);
        Scope<ProductFiscalProfile>(modelBuilder);
        Scope<ServiceFiscalProfile>(modelBuilder);
        Scope<FiscalPreparation>(modelBuilder);
        Scope<FiscalSequence>(modelBuilder);
        Scope<FiscalDocument>(modelBuilder);
        Scope<FiscalEvent>(modelBuilder);

        // Every operational index starts with TenantId, including uniqueness constraints.
        foreach (var entity in modelBuilder.Model.GetEntityTypes().Where(x => typeof(ITenantScoped).IsAssignableFrom(x.ClrType)).ToList())
        {
            foreach (var index in entity.GetIndexes().Where(x => x.Properties.All(p => p.Name != "TenantId")).ToList())
            {
                var names = new[] { "TenantId" }.Concat(index.Properties.Select(x => x.Name)).ToArray();
                var unique = index.IsUnique;
                var filter = index.GetFilter();

                entity.RemoveIndex(index);
                var replacement = modelBuilder.Entity(entity.ClrType).HasIndex(names).IsUnique(unique);
                if (filter is not null)
                {
                    replacement.HasFilter(filter);
                }
            }
        }
    }

    private void Scope<T>(ModelBuilder modelBuilder) where T : class, ITenantScoped
    {
        modelBuilder.Entity<T>().HasQueryFilter(x => TenantId != null && x.TenantId == TenantId);
        modelBuilder.Entity<T>().Property(x => x.TenantId).IsConcurrencyToken();
        if (typeof(T) != typeof(TenantSettings))
        {
            modelBuilder.Entity<T>()
                .HasOne<Tenant>()
                .WithMany()
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    private void ValidateOwnership()
    {
        foreach (var entry in ChangeTracker.Entries<ITenantScoped>().Where(x => x.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            if (TenantId is not { } id)
            {
                throw new InvalidOperationException("An authenticated tenant context is required for writes.");
            }

            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.TenantId != Guid.Empty && entry.Entity.TenantId != id)
                {
                    throw new InvalidOperationException("Cross-tenant writes are forbidden.");
                }

                entry.Entity.TenantId = id;
            }
            else if (entry.Entity.TenantId != id || entry.Property(x => x.TenantId).OriginalValue != id)
            {
                throw new InvalidOperationException("Tenant ownership cannot be changed.");
            }
        }
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ValidateOwnership();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ValidateOwnership();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}
