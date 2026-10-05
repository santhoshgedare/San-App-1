using IdentityHub.Domain.Entities;
using IdentityHub.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityHub.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<Section> Sections => Set<Section>();
    public DbSet<RoleSectionAccess> RoleSectionAccesses => Set<RoleSectionAccess>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<Approval> Approvals => Set<Approval>();
    public DbSet<ApprovalStageDecision> ApprovalStageDecisions => Set<ApprovalStageDecision>();
    public DbSet<ApprovalWorkflow> ApprovalWorkflows => Set<ApprovalWorkflow>();
    public DbSet<ApprovalWorkflowStage> ApprovalWorkflowStages => Set<ApprovalWorkflowStage>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<CategoryVariantDefinition> CategoryVariantDefinitions => Set<CategoryVariantDefinition>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<ItemImage> ItemImages => Set<ItemImage>();
    public DbSet<ItemDocument> ItemDocuments => Set<ItemDocument>();
    public DbSet<ItemVariant> ItemVariants => Set<ItemVariant>();
    public DbSet<ItemReview> ItemReviews => Set<ItemReview>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<PaymentSettings> PaymentSettings => Set<PaymentSettings>();
    public DbSet<Address> Addresses => Set<Address>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.FirstName).HasMaxLength(100);
            entity.Property(u => u.LastName).HasMaxLength(100);
            // Soft-deleted users are excluded from all default queries (UserManager, paged lists, etc.).
            entity.HasQueryFilter(u => !u.IsDeleted);
        });

        builder.Entity<ApplicationRole>(entity =>
        {
            // Soft-deleted roles are excluded from all default queries (RoleManager, paged lists, etc.).
            entity.HasQueryFilter(r => !r.IsDeleted);
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(rt => rt.Id);
            entity.Property(rt => rt.Token).HasMaxLength(256).IsRequired();
            entity.HasIndex(rt => rt.Token).IsUnique();
            entity.HasIndex(rt => rt.UserId);
        });

        builder.Entity<Module>(entity =>
        {
            entity.Property(m => m.Name).HasMaxLength(150).IsRequired();
            entity.Property(m => m.Key).HasMaxLength(150).IsRequired();
            entity.HasIndex(m => m.Key).IsUnique();
            entity.HasMany(m => m.Pages).WithOne(p => p.Module).HasForeignKey(p => p.ModuleId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Page>(entity =>
        {
            entity.Property(p => p.Name).HasMaxLength(150).IsRequired();
            entity.Property(p => p.Url).HasMaxLength(250);
            entity.HasMany(p => p.Sections).WithOne(s => s.Page).HasForeignKey(s => s.PageId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Section>(entity =>
        {
            entity.Property(s => s.Name).HasMaxLength(150).IsRequired();
            entity.Property(s => s.Key).HasMaxLength(150).IsRequired();
            entity.HasIndex(s => s.Key).IsUnique();
            entity.HasMany(s => s.RoleAccess).WithOne(r => r.Section).HasForeignKey(r => r.SectionId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<RoleSectionAccess>(entity =>
        {
            entity.HasIndex(r => new { r.RoleId, r.SectionId }).IsUnique();
        });

        builder.Entity<ActivityLog>(entity =>
        {
            entity.Property(a => a.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(a => a.EntityId).HasMaxLength(100).IsRequired();
            entity.Property(a => a.Action).HasMaxLength(100).IsRequired();
            entity.Property(a => a.Details).HasMaxLength(2000);
            entity.Property(a => a.PerformedByEmail).HasMaxLength(256);
            entity.HasIndex(a => new { a.EntityType, a.EntityId });
            entity.HasIndex(a => a.CreatedAt);
        });

        builder.Entity<Approval>(entity =>
        {
            entity.Property(a => a.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(a => a.EntityId).HasMaxLength(100).IsRequired();
            entity.Property(a => a.Title).HasMaxLength(200).IsRequired();
            entity.Property(a => a.Details).HasMaxLength(2000);
            entity.Property(a => a.RequestedByEmail).HasMaxLength(256);
            entity.Property(a => a.DecidedByEmail).HasMaxLength(256);
            entity.Property(a => a.DecisionComment).HasMaxLength(2000);
            entity.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasIndex(a => new { a.EntityType, a.EntityId });
            entity.HasIndex(a => new { a.EntityType, a.EntityId, a.IsCurrent });
            entity.HasIndex(a => a.Status);
            entity.HasIndex(a => a.RequestedAt);
            entity.HasMany(a => a.StageDecisions).WithOne(d => d.Approval).HasForeignKey(d => d.ApprovalId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ApprovalStageDecision>(entity =>
        {
            entity.Property(d => d.StageName).HasMaxLength(150).IsRequired();
            entity.Property(d => d.AssignedApproverEmail).HasMaxLength(256);
            entity.Property(d => d.Designation).HasMaxLength(150);
            entity.Property(d => d.DecidedByEmail).HasMaxLength(256);
            entity.Property(d => d.Comment).HasMaxLength(2000);
            entity.Property(d => d.Decision).HasConversion<string>().HasMaxLength(20);
        });

        builder.Entity<ApprovalWorkflow>(entity =>
        {
            entity.Property(w => w.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(w => w.Name).HasMaxLength(150).IsRequired();
            entity.HasIndex(w => w.EntityType).IsUnique();
            entity.HasMany(w => w.Stages).WithOne(s => s.Workflow).HasForeignKey(s => s.WorkflowId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ApprovalWorkflowStage>(entity =>
        {
            entity.Property(s => s.Name).HasMaxLength(150).IsRequired();
            entity.Property(s => s.RolesJson).HasMaxLength(2000).IsRequired();
        });

        builder.Entity<Category>(entity =>
        {
            entity.HasQueryFilter(c => !c.IsDeleted);
            entity.Property(c => c.Name).HasMaxLength(150).IsRequired();
            entity.Property(c => c.Description).HasMaxLength(1000);
            entity.Property(c => c.UnitOfMeasurement).HasConversion<string>().HasMaxLength(50);
            entity.HasMany(c => c.VariantDefinitions).WithOne(v => v.Category).HasForeignKey(v => v.CategoryId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(c => c.Name);
            entity.HasIndex(c => c.CreatedAt);
        });

        builder.Entity<CategoryVariantDefinition>(entity =>
        {
            entity.HasQueryFilter(v => !v.Category!.IsDeleted);
            entity.Property(v => v.Name).HasMaxLength(100).IsRequired();
            entity.Property(v => v.Type).HasConversion<string>().HasMaxLength(50);
            entity.Property(v => v.ValuesJson).HasMaxLength(2000).IsRequired();
        });

        builder.Entity<Item>(entity =>
        {
            entity.HasQueryFilter(i => !i.IsDeleted);
            entity.Property(i => i.Code).HasMaxLength(50).IsRequired();
            entity.Property(i => i.Name).HasMaxLength(200).IsRequired();
            entity.Property(i => i.Description).HasMaxLength(2000);
            entity.Property(i => i.Barcode).HasMaxLength(100);
            entity.Property(i => i.UnitOfMeasurement).HasConversion<string>().HasMaxLength(50);
            entity.Property(i => i.Price).HasPrecision(18, 2);
            entity.Property(i => i.CostPrice).HasPrecision(18, 2);

            entity.HasOne(i => i.Category).WithMany().HasForeignKey(i => i.CategoryId).OnDelete(DeleteBehavior.Restrict);
            entity.HasMany(i => i.Images).WithOne(img => img.Item).HasForeignKey(img => img.ItemId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(i => i.Documents).WithOne(doc => doc.Item).HasForeignKey(doc => doc.ItemId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(i => i.Variants).WithOne(v => v.Item).HasForeignKey(v => v.ItemId).OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(i => i.Code).IsUnique();
            entity.HasIndex(i => i.Name);
            entity.HasIndex(i => i.CategoryId);
            entity.HasIndex(i => i.CreatedAt);
        });

        builder.Entity<ItemImage>(entity =>
        {
            entity.HasQueryFilter(img => !img.Item!.IsDeleted);
            entity.Property(img => img.Url).IsRequired();
            entity.Property(img => img.FileName).HasMaxLength(250);
            entity.Property(img => img.Caption).HasMaxLength(250);
        });

        builder.Entity<ItemDocument>(entity =>
        {
            entity.HasQueryFilter(doc => !doc.Item!.IsDeleted);
            entity.Property(doc => doc.Url).IsRequired();
            entity.Property(doc => doc.FileName).HasMaxLength(250).IsRequired();
            entity.Property(doc => doc.DocumentType).HasMaxLength(100);
            entity.Property(doc => doc.Description).HasMaxLength(500);
        });

        builder.Entity<ItemVariant>(entity =>
        {
            entity.HasQueryFilter(v => !v.Item!.IsDeleted);
            entity.Property(v => v.Sku).HasMaxLength(100).IsRequired();
            entity.Property(v => v.Name).HasMaxLength(200);
            entity.Property(v => v.Barcode).HasMaxLength(100);
            entity.Property(v => v.AttributesJson).HasMaxLength(2000).IsRequired();
            entity.Property(v => v.Price).HasPrecision(18, 2);
            entity.Property(v => v.CostPrice).HasPrecision(18, 2);
        });

        builder.Entity<ItemReview>(entity =>
        {
            entity.HasQueryFilter(r => !r.Item!.IsDeleted);
            entity.Property(r => r.ReviewerName).HasMaxLength(200).IsRequired();
            entity.Property(r => r.Title).HasMaxLength(150);
            entity.Property(r => r.Comment).HasMaxLength(2000);

            entity.HasOne(r => r.Item).WithMany().HasForeignKey(r => r.ItemId).OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(r => r.OrderItemId).IsUnique();
            entity.HasIndex(r => new { r.ItemId, r.CreatedAt });
        });

        builder.Entity<Order>(entity =>
        {
            entity.HasQueryFilter(o => !o.IsDeleted);
            entity.Property(o => o.OrderNumber).HasMaxLength(50).IsRequired();
            entity.Property(o => o.CustomerName).HasMaxLength(200).IsRequired();
            entity.Property(o => o.CustomerEmail).HasMaxLength(200).IsRequired();
            entity.Property(o => o.CustomerPhone).HasMaxLength(50).IsRequired();
            entity.Property(o => o.ShippingAddress).HasMaxLength(500).IsRequired();
            entity.Property(o => o.BillingAddress).HasMaxLength(500);
            entity.Property(o => o.OrderNotes).HasMaxLength(1000);
            entity.Property(o => o.PaymentMethod).HasConversion<string>().HasMaxLength(50);
            entity.Property(o => o.PaymentStatus).HasConversion<string>().HasMaxLength(50);
            entity.Property(o => o.PaymentReferenceNumber).HasMaxLength(100);
            entity.Property(o => o.OfflinePaymentNotes).HasMaxLength(1000);
            entity.Property(o => o.RefundAmount).HasPrecision(18, 2);
            entity.Property(o => o.RefundReferenceNumber).HasMaxLength(100);
            entity.Property(o => o.RefundNotes).HasMaxLength(1000);
            entity.Property(o => o.Status).HasConversion<string>().HasMaxLength(50);
            entity.Property(o => o.TrackingNumber).HasMaxLength(100);
            entity.Property(o => o.ShippingCarrier).HasMaxLength(100);
            entity.Property(o => o.SubtotalAmount).HasPrecision(18, 2);
            entity.Property(o => o.ShippingFee).HasPrecision(18, 2);
            entity.Property(o => o.TaxAmount).HasPrecision(18, 2);
            entity.Property(o => o.TotalAmount).HasPrecision(18, 2);

            entity.HasMany(o => o.Items).WithOne(i => i.Order).HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(o => o.OrderNumber).IsUnique();
            entity.HasIndex(o => o.CustomerId);
            entity.HasIndex(o => o.Status);
            entity.HasIndex(o => o.PaymentStatus);
            entity.HasIndex(o => o.CreatedAt);
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.HasQueryFilter(i => !i.Order!.IsDeleted);
            entity.Property(i => i.ItemCode).HasMaxLength(50).IsRequired();
            entity.Property(i => i.ItemName).HasMaxLength(200).IsRequired();
            entity.Property(i => i.VariantSku).HasMaxLength(100);
            entity.Property(i => i.VariantName).HasMaxLength(200);
            entity.Property(i => i.AttributesJson).HasMaxLength(1000);
            entity.Property(i => i.UnitPrice).HasPrecision(18, 2);
            entity.Property(i => i.UnitCostPrice).HasPrecision(18, 2);
            entity.Property(i => i.TotalPrice).HasPrecision(18, 2);

            entity.HasOne(i => i.Item).WithMany().HasForeignKey(i => i.ItemId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(i => i.ItemVariant).WithMany().HasForeignKey(i => i.ItemVariantId).OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<PaymentSettings>(entity =>
        {
            entity.Property(p => p.UpiId).HasMaxLength(200).IsRequired();
            entity.Property(p => p.PayeeName).HasMaxLength(200);
            entity.Property(p => p.Instructions).HasMaxLength(1000);
            entity.Property(p => p.UpdatedByEmail).HasMaxLength(256);
        });

        builder.Entity<Address>(entity =>
        {
            entity.HasQueryFilter(a => !a.IsDeleted);
            entity.Property(a => a.Label).HasMaxLength(100).IsRequired();
            entity.Property(a => a.FullName).HasMaxLength(200).IsRequired();
            entity.Property(a => a.Phone).HasMaxLength(50).IsRequired();
            entity.Property(a => a.Line1).HasMaxLength(250).IsRequired();
            entity.Property(a => a.Line2).HasMaxLength(250);
            entity.Property(a => a.City).HasMaxLength(100).IsRequired();
            entity.Property(a => a.State).HasMaxLength(100).IsRequired();
            entity.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
            entity.Property(a => a.Country).HasMaxLength(100).IsRequired();
            entity.Property(a => a.FormattedAddress).HasMaxLength(500);
            entity.HasIndex(a => a.UserId);
        });
    }
}


