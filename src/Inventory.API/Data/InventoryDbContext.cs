using Inventory.API.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.API.Data;

public sealed class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<InventoryItemPromotion> InventoryPromotions => Set<InventoryItemPromotion>();
    public DbSet<InventoryOutbox> InventoryOutbox => Set<InventoryOutbox>();

    public DbSet<Restaurant> Restaurants => Set<Restaurant>();
    public DbSet<Pharmacy> Pharmacies => Set<Pharmacy>();
    public DbSet<Shop> Shops => Set<Shop>();

    public DbSet<Retailer> Retailers => Set<Retailer>();
    public DbSet<Branch> Branches => Set<Branch>();

    public DbSet<Variety> Varieties => Set<Variety>();

    public DbSet<Menu> Menus => Set<Menu>();
    public DbSet<InventoryReadModel> InventoryViews => Set<InventoryReadModel>();
    public DbSet<MenuReadModel> MenuViews => Set<MenuReadModel>();
    public DbSet<BranchReadModel> BranchViews => Set<BranchReadModel>();
    public DbSet<AuditLogEntry> AuditLog => Set<AuditLogEntry>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Timetable> Timetables => Set<Timetable>();
    public DbSet<MenuHistory> MenuHistories => Set<MenuHistory>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InventoryItem>(e =>
        {
            e.ToTable("inventory_item");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.RetailerId);
            e.HasIndex(x => x.ItemType);
            e.HasIndex(x => x.InventoryItemCode)
                .IsUnique()
                .HasFilter("inventory_item_code IS NOT NULL");
            e.Property(x => x.RowVersion).IsConcurrencyToken();
            e.Property(x => x.OpeningDayHours).HasColumnType("jsonb");
            e.Property(x => x.DisplayTimes).HasColumnType("jsonb");
        });

        modelBuilder.Entity<InventoryItemPromotion>(e =>
        {
            e.ToTable("inventory_item_promotion");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.OwnerId);
            e.Property(x => x.InventoryItemIds).HasColumnType("jsonb");
            e.Property(x => x.MenuIds).HasColumnType("jsonb");
        });

        modelBuilder.Entity<InventoryOutbox>(e =>
        {
            e.ToTable("inventory_outbox");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.OccurredAt)
                .HasFilter("processed_at IS NULL")
                .HasDatabaseName("idx_outbox_unprocessed");
            e.Property(x => x.Payload).HasColumnType("jsonb");
        });

        modelBuilder.Entity<Restaurant>(e =>
        {
            e.ToTable("restaurants");
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<Pharmacy>(e =>
        {
            e.ToTable("pharmacies");
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<Shop>(e =>
        {
            e.ToTable("shops");
            e.HasKey(x => x.Id);
        });

        modelBuilder.Entity<Retailer>(e =>
        {
            e.ToTable("retailers");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.RetailerType);
            e.Property(x => x.OrderTelephoneNumbers).HasColumnType("jsonb");
            e.Property(x => x.Stations).HasColumnType("jsonb");
            e.Property(x => x.StationsIds).HasColumnType("jsonb");
            e.Property(x => x.PaymentMethods).HasColumnType("jsonb");
            e.Property(x => x.PreferredPaymentMethods).HasColumnType("jsonb");
            e.Property(x => x.OpeningDayHours).HasColumnType("jsonb");
            e.Property(x => x.DisplayTimes).HasColumnType("jsonb");
            e.Property(x => x.RestaurantImages).HasColumnType("jsonb");
            e.Property(x => x.SearchTerms).HasColumnType("jsonb");
            e.Property(x => x.RetailerAgreement).HasColumnType("jsonb");
            e.Property(x => x.SocialMediaLinks).HasColumnType("jsonb");
            e.Property(x => x.MealTypes).HasColumnType("jsonb");
            e.Property(x => x.PreparationStyle).HasColumnType("jsonb");
            e.Property(x => x.MealPackaging).HasColumnType("jsonb");
        });

        modelBuilder.Entity<Branch>(e =>
        {
            e.ToTable("branches");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.RetailerId);
            e.HasIndex(x => x.RetailerType);
            e.Property(x => x.RowVersion).IsConcurrencyToken();
            e.Property(x => x.Stations).HasColumnType("jsonb");
            e.Property(x => x.StationsIds).HasColumnType("jsonb");
            e.Property(x => x.PaymentMethods).HasColumnType("jsonb");
            e.Property(x => x.PreferredPaymentMethods).HasColumnType("jsonb");
            e.Property(x => x.OpeningDayHours).HasColumnType("jsonb");
            e.Property(x => x.DisplayTimes).HasColumnType("jsonb");
            e.Property(x => x.RestaurantImages).HasColumnType("jsonb");
            e.Property(x => x.SearchTerms).HasColumnType("jsonb");
            e.Property(x => x.MealTypes).HasColumnType("jsonb");
        });

        modelBuilder.Entity<Variety>(e =>
        {
            e.ToTable("variety");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.OwnerId);
            e.Property(x => x.InventoryItemIds).HasColumnType("jsonb");
            e.Property(x => x.Varieties).HasColumnType("jsonb");
        });

        modelBuilder.Entity<AuditLogEntry>(e =>
        {
            e.ToTable("audit_log");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.AggregateId);
            e.HasIndex(x => x.OccurredAt);
            e.HasIndex(x => x.OutboxId).IsUnique();
            e.Property(x => x.BeforeJson).HasColumnType("jsonb");
            e.Property(x => x.AfterJson).HasColumnType("jsonb");
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("category");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.OwnerId);
        });

        modelBuilder.Entity<Menu>(e =>
        {
            e.ToTable("menu");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.OwnerId);
            e.HasIndex(x => x.MenuItemCode)
                .IsUnique()
                .HasFilter("menu_item_code IS NOT NULL");
            e.Property(x => x.RowVersion).IsConcurrencyToken();
            e.Property(x => x.InventoryItemIds).HasColumnType("jsonb");
        });

        modelBuilder.Entity<Timetable>(e =>
        {
            e.ToTable("timetable");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.OwnerId);
            e.Property(x => x.Openings).HasColumnType("jsonb");
        });

        modelBuilder.Entity<MenuHistory>(e =>
        {
            e.ToTable("menu_history");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.MenuId);
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.ToTable("orders");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.RetailerId);
            e.HasIndex(x => x.Status);
            e.HasMany(x => x.Lines)
                .WithOne(x => x.Order)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderLine>(e =>
        {
            e.ToTable("order_lines");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.OrderId);
        });

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.HasIndex(x => x.RetailerId);
        });

        modelBuilder.Entity<InventoryReadModel>(e =>
        {
            e.ToView("inventory_view");
            e.HasNoKey();
            e.Property(x => x.OpeningDayHours).HasColumnType("jsonb");
            e.Property(x => x.DisplayTimes).HasColumnType("jsonb");
        });

        modelBuilder.Entity<MenuReadModel>(e =>
        {
            e.ToView("menu_view");
            e.HasNoKey();
            e.Ignore(x => x.InventoryItems);
        });

        modelBuilder.Entity<BranchReadModel>(e =>
        {
            e.ToView("branch_view");
            e.HasNoKey();
        });
    }
}
