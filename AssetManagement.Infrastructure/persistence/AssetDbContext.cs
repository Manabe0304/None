using AssetManagement.Domain.entities;
using AssetManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssetManagement.Infrastructure.Persistence
{
    public class AssetDbContext : DbContext
    {
        public AssetDbContext()
        {
        }

        public AssetDbContext(DbContextOptions<AssetDbContext> options)
            : base(options)
        {
        }

        public override int SaveChanges()
        {
            ConvertUtc();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ConvertUtc();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void ConvertUtc()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                foreach (var property in entry.Properties)
                {
                    if (property.CurrentValue is DateTime dt)
                    {
                        property.CurrentValue = dt.Kind == DateTimeKind.Utc
                            ? dt
                            : DateTime.SpecifyKind(dt, DateTimeKind.Utc);
                    }
                }
            }
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<AssetRequest> AssetRequests { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
        public DbSet<AssetWarranty> AssetWarranties { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<ImportBatch> ImportBatches { get; set; }
        public DbSet<AssetReturnRequest> ReturnRequests { get; set; }
        public DbSet<BrokenReport> BrokenReports { get; set; }
        public DbSet<AssetLiquidation> Liquidations { get; set; }
        public DbSet<AssetHistory> AssetHistories { get; set; }
        public DbSet<AssetInspection> AssetInspections { get; set; }
        public DbSet<AssetReservation> AssetReservations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            modelBuilder.Entity<Department>()
                .HasIndex(d => d.Code)
                .IsUnique();

            modelBuilder.Entity<Asset>()
                .HasIndex(a => a.AssetTag)
                .IsUnique();

            modelBuilder.Entity<Asset>()
                .HasIndex(a => a.SerialNumber)
                .IsUnique();

            modelBuilder.Entity<AssetLiquidation>()
                .HasIndex(l => l.AssetId)
                .IsUnique();

            modelBuilder.Entity<AssetReservation>()
                .HasIndex(r => new { r.AssetId, r.Status })
                .IsUnique()
                .HasDatabaseName("uq_reservation_asset");

            modelBuilder.Entity<Asset>()
                .Property(a => a.PurchaseValue)
                .HasPrecision(18, 2);

            modelBuilder.Entity<MaintenanceRecord>()
                .Property(m => m.EstimatedCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<MaintenanceRecord>()
                .Property(m => m.RepairCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Department>()
                .HasOne(d => d.Manager)
                .WithMany()
                .HasForeignKey(d => d.ManagerId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId);

            modelBuilder.Entity<Asset>()
                .HasOne(a => a.Department)
                .WithMany(d => d.Assets)
                .HasForeignKey(a => a.DepartmentId);

            modelBuilder.Entity<Asset>()
                .HasOne(a => a.CurrentUser)
                .WithMany()
                .HasForeignKey(a => a.CurrentUserId);

            modelBuilder.Entity<AssetRequest>()
                .HasOne(ar => ar.Employee)
                .WithMany(u => u.AssetRequests)
                .HasForeignKey(ar => ar.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AssetRequest>()
                .HasOne(ar => ar.Manager)
                .WithMany()
                .HasForeignKey(ar => ar.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.User)
                .WithMany(u => u.Assignments)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.AssignedBy)
                .WithMany()
                .HasForeignKey(a => a.AssignedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Asset)
                .WithMany(a => a.Assignments)
                .HasForeignKey(a => a.AssetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Request)
                .WithMany(r => r.Assignments)
                .HasForeignKey(a => a.RequestId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<MaintenanceRecord>()
                .HasOne(m => m.Asset)
                .WithMany(a => a.MaintenanceRecords)
                .HasForeignKey(m => m.AssetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MaintenanceRecord>()
                .HasOne(m => m.Reporter)
                .WithMany()
                .HasForeignKey(m => m.ReportedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaintenanceRecord>()
                .HasOne(m => m.Vendor)
                .WithMany(v => v.MaintenanceRecords)
                .HasForeignKey(m => m.VendorId);

            modelBuilder.Entity<MaintenanceRecord>()
                .HasOne(m => m.BrokenReport)
                .WithMany()
                .HasForeignKey(m => m.BrokenReportId);

            modelBuilder.Entity<AssetWarranty>()
                .HasOne(w => w.Asset)
                .WithMany(a => a.Warranties)
                .HasForeignKey(w => w.AssetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(a => a.Id);

                entity.Property(a => a.Action);

                entity.Property(a => a.EntityType);

                entity.Property(a => a.CreatedAt)
                    .IsRequired();

                entity.Property(a => a.ActorEmail)
                    .HasMaxLength(256);

                entity.Property(a => a.ActorDepartment)
                    .HasMaxLength(256);

                entity.Property(a => a.TargetEmail)
                    .HasMaxLength(256);

                entity.HasOne(a => a.User)
                    .WithMany()
                    .HasForeignKey(a => a.UserId);
            });

            modelBuilder.Entity<ImportBatch>()
                .HasOne(i => i.ImportedByUser)
                .WithMany()
                .HasForeignKey(i => i.ImportedBy);

            modelBuilder.Entity<ImportBatch>()
                .HasOne(i => i.User)
                .WithMany()
                .HasForeignKey(i => i.UserId);

            modelBuilder.Entity<AssetReturnRequest>()
                .HasOne(r => r.Assignment)
                .WithMany(a => a.ReturnRequests)
                .HasForeignKey(r => r.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AssetReturnRequest>()
                .HasOne(r => r.Asset)
                .WithMany(a => a.ReturnRequests)
                .HasForeignKey(r => r.AssetId);

            modelBuilder.Entity<AssetReturnRequest>()
                .HasOne(r => r.User)
                .WithMany(u => u.ReturnRequests)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AssetReturnRequest>()
                .HasOne(r => r.InitiatedBy)
                .WithMany(u => u.InitiatedReturnRequests)
                .HasForeignKey(r => r.InitiatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AssetReturnRequest>()
                .HasOne(r => r.HandledBy)
                .WithMany()
                .HasForeignKey(r => r.HandledById);

            modelBuilder.Entity<BrokenReport>()
                .HasOne(b => b.Asset)
                .WithMany(a => a.BrokenReports)
                .HasForeignKey(b => b.AssetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BrokenReport>()
                .HasOne(b => b.ReportedBy)
                .WithMany(u => u.BrokenReports)
                .HasForeignKey(b => b.ReportedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BrokenReport>()
                .HasOne(b => b.TriageBy)
                .WithMany()
                .HasForeignKey(b => b.TriageById);

            modelBuilder.Entity<BrokenReport>()
                .HasOne(b => b.MaintenanceRecord)
                .WithMany()
                .HasForeignKey(b => b.MaintenanceId);

            modelBuilder.Entity<AssetLiquidation>()
                .HasOne(l => l.Asset)
                .WithOne(a => a.Liquidation)
                .HasForeignKey<AssetLiquidation>(l => l.AssetId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AssetLiquidation>()
                .HasOne(l => l.LiquidatedBy)
                .WithMany(u => u.Liquidations)
                .HasForeignKey(l => l.LiquidatedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AssetHistory>()
                .HasOne(h => h.Asset)
                .WithMany(a => a.Histories)
                .HasForeignKey(h => h.AssetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AssetHistory>()
                .HasOne(h => h.Assignment)
                .WithMany(a => a.Histories)
                .HasForeignKey(h => h.AssignmentId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AssetHistory>()
                .HasOne(h => h.User)
                .WithMany(u => u.AssetHistories)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AssetHistory>()
                .HasOne(h => h.ChangedBy)
                .WithMany(u => u.ChangedAssetHistories)
                .HasForeignKey(h => h.ChangedById)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AssetInspection>()
                .HasOne(i => i.Asset)
                .WithMany(a => a.Inspections)
                .HasForeignKey(i => i.AssetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AssetInspection>()
                .HasOne(i => i.ReturnRequest)
                .WithMany()
                .HasForeignKey(i => i.ReturnRequestId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<AssetInspection>()
                .HasOne(i => i.InspectedBy)
                .WithMany(u => u.AssetInspections)
                .HasForeignKey(i => i.InspectedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AssetReservation>()
                .HasOne(r => r.Asset)
                .WithMany(a => a.Reservations)
                .HasForeignKey(r => r.AssetId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AssetReservation>()
                .HasOne(r => r.Request)
                .WithMany(a => a.Reservations)
                .HasForeignKey(r => r.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AssetReservation>()
                .HasOne(r => r.ReservedBy)
                .WithMany(u => u.AssetReservations)
                .HasForeignKey(r => r.ReservedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}