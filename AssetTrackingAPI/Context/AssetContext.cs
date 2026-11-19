using Microsoft.EntityFrameworkCore;
using AssetTrackingAPI.Domain;

namespace AssetTrackingAPI.Context;

public class AssetContext : DbContext 
{
    public AssetContext(DbContextOptions<AssetContext> options) : base(options) {} 

    public DbSet<User> Users => Set<User>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<EpiType> EpiTypes => Set<EpiType>();
    public DbSet<EpiItem> EpiItems => Set<EpiItem>();
    public DbSet<ApprovalCertificate> ApprovalCertificates => Set<ApprovalCertificate>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();
    public DbSet<DeliveryItem> DeliveryItems => Set<DeliveryItem>();
    public DbSet<ReturnRecord> Returns => Set<ReturnRecord>();
    public DbSet<Inspection> Inspections => Set<Inspection>();
    public DbSet<ComplianceAlert> ComplianceAlerts => Set<ComplianceAlert>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();
        modelBuilder.Entity<EpiItem>().HasIndex(x => x.UniqueCode).IsUnique();
        modelBuilder.Entity<EpiItem>().Property(x => x.Status).HasConversion<string>();
        modelBuilder.Entity<StockMovement>().Property(x => x.Type).HasConversion<string>();
        modelBuilder.Entity<Inspection>().Property(x => x.Condition).HasConversion<string>();
    }
}