using Microsoft.EntityFrameworkCore;
using Maba.Application.Common.Interfaces;
using Maba.Domain.Common;
using Maba.Domain.Users;
using Maba.Domain.Media;
using Maba.Domain.Catalog;
using Maba.Domain.Machines;
using Maba.Domain.Printing;
using Maba.Domain.Orders;
using Maba.Domain.Finance;
using Maba.Domain.Cms;
using Maba.Domain.AiChat;
using Maba.Domain.SupportChat;
using Maba.Domain.Laser;
using Maba.Domain.Software;
using Maba.Domain.Cnc;
using Maba.Domain.Projects;
using Maba.Domain.Faq;
using Maba.Domain.HeroTicker;
using Maba.Domain.Design;
using Maba.Domain.DesignCad;
using Maba.Domain.ControlCenter;
using Maba.Domain.Lookups;
using Maba.Domain.Crm;
using Maba.Domain.Pricing;
using Maba.Domain.Inventory;
using Maba.Domain.Numbering;
using Maba.Domain.Tax;
using Maba.Domain.Accounting;
using Maba.Domain.CommercialReturns;
using Maba.Domain.Sales;
using Maba.Infrastructure.Data.Configurations;
using CatalogInventory = Maba.Domain.Catalog.Inventory;

namespace Maba.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Users & Access
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    // Media
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
    public DbSet<MediaType> MediaTypes => Set<MediaType>();
    public DbSet<MediaUsageType> MediaUsageTypes => Set<MediaUsageType>();
    public DbSet<EntityMediaLink> EntityMediaLinks => Set<EntityMediaLink>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();

    // Catalog
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<ItemStatus> ItemStatuses => Set<ItemStatus>();
    public DbSet<ItemTag> ItemTags => Set<ItemTag>();
    public DbSet<CatalogInventory> Inventories => Set<CatalogInventory>();
    public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
    public DbSet<ItemSection> ItemSections => Set<ItemSection>();
    public DbSet<ItemSectionFeature> ItemSectionFeatures => Set<ItemSectionFeature>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<UnitOfMeasure> UnitsOfMeasure => Set<UnitOfMeasure>();
    public DbSet<UnitConversion> UnitConversions => Set<UnitConversion>();
    public DbSet<ItemUnit> ItemUnits => Set<ItemUnit>();
    public DbSet<SupplierItemPrice> SupplierItemPrices => Set<SupplierItemPrice>();

    // Lookups / CRM / Pricing / Inventory foundation
    public DbSet<LookupType> LookupTypes => Set<LookupType>();
    public DbSet<LookupValue> LookupValues => Set<LookupValue>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<PriceListItem> PriceListItems => Set<PriceListItem>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<InventoryCostLayer> InventoryCostLayers => Set<InventoryCostLayer>();
    public DbSet<DocumentSequence> DocumentSequences => Set<DocumentSequence>();
    public DbSet<TaxConfiguration> TaxConfigurations => Set<TaxConfiguration>();
    public DbSet<AccountType> AccountTypes => Set<AccountType>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<FiscalYear> FiscalYears => Set<FiscalYear>();
    public DbSet<FiscalPeriod> FiscalPeriods => Set<FiscalPeriod>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
    public DbSet<SalesReturn> SalesReturns => Set<SalesReturn>();
    public DbSet<SalesReturnLine> SalesReturnLines => Set<SalesReturnLine>();
    public DbSet<PurchaseReturn> PurchaseReturns => Set<PurchaseReturn>();
    public DbSet<PurchaseReturnLine> PurchaseReturnLines => Set<PurchaseReturnLine>();

    // Machines
    public DbSet<Machine> Machines => Set<Machine>();
    public DbSet<MachinePart> MachineParts => Set<MachinePart>();
    public DbSet<ItemMachineLink> ItemMachineLinks => Set<ItemMachineLink>();

    // 3D Printing
    public DbSet<Material> Materials => Set<Material>();
    public DbSet<MaterialColor> MaterialColors => Set<MaterialColor>();
    public DbSet<PrintingTechnology> PrintingTechnologies => Set<PrintingTechnology>();
    public DbSet<Printer> Printers => Set<Printer>();
    public DbSet<SlicingProfile> SlicingProfiles => Set<SlicingProfile>();
    public DbSet<PrintQualityProfile> PrintQualityProfiles => Set<PrintQualityProfile>();
    public DbSet<Design> Designs => Set<Design>();
    public DbSet<Print3dServiceRequest> Print3dServiceRequests => Set<Print3dServiceRequest>();
    public DbSet<FilamentSpool> FilamentSpools => Set<FilamentSpool>();
    public DbSet<DesignFile> DesignFiles => Set<DesignFile>();
    public DbSet<SlicingJob> SlicingJobs => Set<SlicingJob>();
    public DbSet<SlicingJobStatus> SlicingJobStatuses => Set<SlicingJobStatus>();
    public DbSet<PrintJob> PrintJobs => Set<PrintJob>();
    public DbSet<PrintJobStatus> PrintJobStatuses => Set<PrintJobStatus>();

    // Orders
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderStatus> OrderStatuses => Set<OrderStatus>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceStatus> InvoiceStatuses => Set<InvoiceStatus>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<PaymentPlan> PaymentPlans => Set<PaymentPlan>();
    public DbSet<Installment> Installments => Set<Installment>();
    public DbSet<InstallmentStatus> InstallmentStatuses => Set<InstallmentStatus>();

    // Sales / Quotations
    public DbSet<Quotation> Quotations => Set<Quotation>();
    public DbSet<QuotationItem> QuotationItems => Set<QuotationItem>();

    // Finance
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();
    public DbSet<Income> Incomes => Set<Income>();
    public DbSet<IncomeSource> IncomeSources => Set<IncomeSource>();

    // CMS
    public DbSet<Page> Pages => Set<Page>();
    public DbSet<PageTemplate> PageTemplates => Set<PageTemplate>();
    public DbSet<PageVersion> PageVersions => Set<PageVersion>();
    public DbSet<PageSectionType> PageSectionTypes => Set<PageSectionType>();
    public DbSet<LayoutType> LayoutTypes => Set<LayoutType>();
    public DbSet<PageSectionDraft> PageSectionDrafts => Set<PageSectionDraft>();
    public DbSet<PageSectionItemDraft> PageSectionItemDrafts => Set<PageSectionItemDraft>();
    public DbSet<PageSectionPublished> PageSectionPublished => Set<PageSectionPublished>();
    public DbSet<PageSectionItemPublished> PageSectionItemPublished => Set<PageSectionItemPublished>();

    // AI Chat
    public DbSet<AiSession> AiSessions => Set<AiSession>();
    public DbSet<AiSessionSource> AiSessionSources => Set<AiSessionSource>();
    public DbSet<AiSenderType> AiSenderTypes => Set<AiSenderType>();
    public DbSet<AiMessage> AiMessages => Set<AiMessage>();
    public DbSet<AiChatConfig> AiChatConfigs => Set<AiChatConfig>();

    // Support Chat (live chat customer ↔ admin)
    public DbSet<SupportConversation> SupportConversations => Set<SupportConversation>();
    public DbSet<SupportMessage> SupportMessages => Set<SupportMessage>();

    // Laser
    public DbSet<LaserMaterial> LaserMaterials => Set<LaserMaterial>();
    public DbSet<LaserServiceRequest> LaserServiceRequests => Set<LaserServiceRequest>();

    // Software
    public DbSet<SoftwareProduct> SoftwareProducts => Set<SoftwareProduct>();
    public DbSet<SoftwareRelease> SoftwareReleases => Set<SoftwareRelease>();
    public DbSet<SoftwareFile> SoftwareFiles => Set<SoftwareFile>();

    // Projects
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectRequest> ProjectRequests => Set<ProjectRequest>();
    public DbSet<ProjectRequestActivity> ProjectRequestActivities => Set<ProjectRequestActivity>();

    // CNC
    public DbSet<CncMaterial> CncMaterials => Set<CncMaterial>();
    public DbSet<CncServiceRequest> CncServiceRequests => Set<CncServiceRequest>();

    // Design & CAD
    public DbSet<DesignServiceRequest> DesignServiceRequests => Set<DesignServiceRequest>();
    public DbSet<DesignServiceRequestAttachment> DesignServiceRequestAttachments => Set<DesignServiceRequestAttachment>();

    // Design CAD (landing + request flow)
    public DbSet<DesignCadServiceRequest> DesignCadServiceRequests => Set<DesignCadServiceRequest>();
    public DbSet<DesignCadServiceRequestAttachment> DesignCadServiceRequestAttachments => Set<DesignCadServiceRequestAttachment>();

    // Cross-cutting
    public DbSet<FaqItem> FaqItems => Set<FaqItem>();
    public DbSet<HeroTickerItem> HeroTickerItems => Set<HeroTickerItem>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
    public DbSet<Notification> Notifications => Set<Notification>();

    // Control Center
    public DbSet<ControlCenterInstance> ControlCenterInstances => Set<ControlCenterInstance>();
    public DbSet<CcDevice> CcDevices => Set<CcDevice>();
    public DbSet<CcJobTemplate> CcJobTemplates => Set<CcJobTemplate>();
    public DbSet<CcJob> CcJobs => Set<CcJob>();
    public DbSet<CcCommand> CcCommands => Set<CcCommand>();
    public DbSet<CcTelemetryRecord> CcTelemetryRecords => Set<CcTelemetryRecord>();
    public DbSet<CcAuditEvent> CcAuditEvents => Set<CcAuditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure global query filters if needed
        // e.g., modelBuilder.Entity<Item>().HasQueryFilter(i => i.IsActive);
        
        // Configure decimal precision for SQL Server
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                {
                    property.SetColumnType("decimal(18,2)");
                }
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update UpdatedAt for entities that have changed
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is BaseEntity && 
                       (e.State == EntityState.Modified || e.State == EntityState.Added));

        foreach (var entry in entries)
        {
            var entity = (BaseEntity)entry.Entity;
            
            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTime.UtcNow;
            }
            
            entity.UpdatedAt = DateTime.UtcNow;
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

