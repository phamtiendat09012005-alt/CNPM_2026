using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SportsStoreAI.API.Models;

namespace SportsStoreAI.API.Data;

public sealed class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<UserAddress> UserAddresses => Set<UserAddress>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<ProductViewHistory> ProductViewHistories => Set<ProductViewHistory>();
    public DbSet<SizeChart> SizeCharts => Set<SizeChart>();
    public DbSet<RecommendationLog> RecommendationLogs => Set<RecommendationLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        ConfigureIdentity(builder);
        ConfigureCatalog(builder);
        ConfigureShopping(builder);
        ConfigureOrders(builder);
        ConfigureRecommendations(builder);
    }

    private static void ConfigureIdentity(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnType("datetime2(0)");
            entity.Property(x => x.UpdatedAt).HasColumnType("datetime2(0)");
        });

        builder.Entity<IdentityRole<int>>().ToTable("Roles");
        builder.Entity<IdentityUserRole<int>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
        builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");
    }

    private static void ConfigureCatalog(ModelBuilder builder)
    {
        builder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.CreatedAt).HasColumnType("datetime2(0)");
            entity.Property(x => x.UpdatedAt).HasColumnType("datetime2(0)");
            entity.HasIndex(x => x.Name).IsUnique();
        });

        builder.Entity<Brand>(entity =>
        {
            entity.ToTable("Brands");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LogoUrl).HasMaxLength(500);
            entity.Property(x => x.CreatedAt).HasColumnType("datetime2(0)");
            entity.Property(x => x.UpdatedAt).HasColumnType("datetime2(0)");
            entity.HasIndex(x => x.Name).IsUnique();
        });

        builder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Slug).HasMaxLength(220).IsRequired();
            entity.Property(x => x.BasePrice).HasPrecision(18, 2);
            entity.Property(x => x.SportType).HasMaxLength(80);
            entity.Property(x => x.ProductType).HasMaxLength(30).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnType("datetime2(0)");
            entity.Property(x => x.UpdatedAt).HasColumnType("datetime2(0)");
            entity.HasIndex(x => x.Slug).IsUnique();

            entity.HasOne(x => x.Category)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Brand)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.BrandId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ProductImage>(entity =>
        {
            entity.ToTable("ProductImages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Url).HasMaxLength(500).IsRequired();

            entity.HasOne(x => x.Product)
                .WithMany(x => x.ProductImages)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductVariant>(entity =>
        {
            entity.ToTable("ProductVariants");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Sku).HasColumnName("SKU").HasMaxLength(60).IsRequired();
            entity.Property(x => x.Size).HasMaxLength(30);
            entity.Property(x => x.Color).HasMaxLength(50);
            entity.Property(x => x.Price).HasPrecision(18, 2);
            entity.Property(x => x.CreatedAt).HasColumnType("datetime2(0)");
            entity.Property(x => x.UpdatedAt).HasColumnType("datetime2(0)");
            entity.HasIndex(x => x.Sku).IsUnique();

            entity.HasOne(x => x.Product)
                .WithMany(x => x.ProductVariants)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureShopping(ModelBuilder builder)
    {
        builder.Entity<UserAddress>(entity =>
        {
            entity.ToTable("UserAddresses");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ReceiverName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.ReceiverPhone).HasMaxLength(20).IsRequired();
            entity.Property(x => x.AddressLine).HasMaxLength(300).IsRequired();
            entity.Property(x => x.Ward).HasMaxLength(100).IsRequired();
            entity.Property(x => x.District).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Province).HasMaxLength(100).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnType("datetime2(0)");
            entity.Property(x => x.UpdatedAt).HasColumnType("datetime2(0)");

            entity.HasOne(x => x.User)
                .WithMany(x => x.Addresses)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Cart>(entity =>
        {
            entity.ToTable("Carts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CreatedAt).HasColumnType("datetime2(0)");
            entity.Property(x => x.UpdatedAt).HasColumnType("datetime2(0)");
            entity.HasIndex(x => x.UserId).IsUnique();

            entity.HasOne(x => x.User)
                .WithOne(x => x.Cart)
                .HasForeignKey<Cart>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CartItem>(entity =>
        {
            entity.ToTable("CartItems");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.AddedAt).HasColumnType("datetime2(0)");
            entity.Property(x => x.UpdatedAt).HasColumnType("datetime2(0)");
            entity.HasIndex(x => new { x.CartId, x.ProductVariantId }).IsUnique();

            entity.HasOne(x => x.Cart)
                .WithMany(x => x.CartItems)
                .HasForeignKey(x => x.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.ProductVariant)
                .WithMany(x => x.CartItems)
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureOrders(ModelBuilder builder)
    {
        builder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OrderCode).HasMaxLength(30).IsRequired();
            entity.Property(x => x.ReceiverName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.ReceiverPhone).HasMaxLength(20).IsRequired();
            entity.Property(x => x.ShippingAddress).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Subtotal).HasPrecision(18, 2);
            entity.Property(x => x.ShippingFee).HasPrecision(18, 2);
            entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Note).HasMaxLength(500);
            entity.Property(x => x.CreatedAt).HasColumnType("datetime2(0)");
            entity.Property(x => x.UpdatedAt).HasColumnType("datetime2(0)");
            entity.Property(x => x.CancelledAt).HasColumnType("datetime2(0)");
            entity.HasIndex(x => x.OrderCode).IsUnique();

            entity.HasOne(x => x.User)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProductName).HasMaxLength(180).IsRequired();
            entity.Property(x => x.Sku).HasColumnName("SKU").HasMaxLength(60).IsRequired();
            entity.Property(x => x.Size).HasMaxLength(30);
            entity.Property(x => x.Color).HasMaxLength(50);
            entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
            entity.Property(x => x.TotalPrice)
                .HasComputedColumnSql("([UnitPrice]*[Quantity])", stored: true);

            entity.HasOne(x => x.Order)
                .WithMany(x => x.OrderItems)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.ProductVariant)
                .WithMany(x => x.OrderItems)
                .HasForeignKey(x => x.ProductVariantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Method).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
            entity.Property(x => x.Amount).HasPrecision(18, 2);
            entity.Property(x => x.Provider).HasMaxLength(60);
            entity.Property(x => x.ProviderTransactionId).HasMaxLength(120);
            entity.Property(x => x.PaymentUrl).HasMaxLength(1000);
            entity.Property(x => x.FailureReason).HasMaxLength(500);
            entity.Property(x => x.PaidAt).HasColumnType("datetime2(0)");
            entity.Property(x => x.CreatedAt).HasColumnType("datetime2(0)");
            entity.Property(x => x.UpdatedAt).HasColumnType("datetime2(0)");

            entity.HasOne(x => x.Order)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<OrderStatusHistory>(entity =>
        {
            entity.ToTable("OrderStatusHistories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Note).HasMaxLength(500);
            entity.Property(x => x.ChangedAt).HasColumnType("datetime2(0)");

            entity.HasOne(x => x.Order)
                .WithMany(x => x.OrderStatusHistories)
                .HasForeignKey(x => x.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.ChangedByUser)
                .WithMany()
                .HasForeignKey(x => x.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureRecommendations(ModelBuilder builder)
    {
        builder.Entity<ProductViewHistory>(entity =>
        {
            entity.ToTable("ProductViewHistories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SessionId).HasMaxLength(100);
            entity.Property(x => x.ViewedAt).HasColumnType("datetime2(0)");

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Product)
                .WithMany(x => x.ViewHistories)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SizeChart>(entity =>
        {
            entity.ToTable("SizeCharts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ProductType).HasMaxLength(30).IsRequired();
            entity.Property(x => x.SizeLabel).HasMaxLength(30).IsRequired();
            entity.Property(x => x.FitNote).HasMaxLength(300);
            entity.Property(x => x.MinFootLengthCm).HasPrecision(5, 2);
            entity.Property(x => x.MaxFootLengthCm).HasPrecision(5, 2);
            entity.Property(x => x.MinHeightCm).HasPrecision(5, 2);
            entity.Property(x => x.MaxHeightCm).HasPrecision(5, 2);
            entity.Property(x => x.MinWeightKg).HasPrecision(5, 2);
            entity.Property(x => x.MaxWeightKg).HasPrecision(5, 2);
            entity.Property(x => x.MinChestCm).HasPrecision(5, 2);
            entity.Property(x => x.MaxChestCm).HasPrecision(5, 2);
            entity.Property(x => x.MinWaistCm).HasPrecision(5, 2);
            entity.Property(x => x.MaxWaistCm).HasPrecision(5, 2);
            entity.Property(x => x.CreatedAt).HasColumnType("datetime2(0)");
            entity.Property(x => x.UpdatedAt).HasColumnType("datetime2(0)");

            entity.HasOne(x => x.Brand)
                .WithMany(x => x.SizeCharts)
                .HasForeignKey(x => x.BrandId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<RecommendationLog>(entity =>
        {
            entity.ToTable("RecommendationLogs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SessionId).HasMaxLength(100);
            entity.Property(x => x.RecommendationType).HasMaxLength(30).IsRequired();
            entity.Property(x => x.RecommendedSize).HasMaxLength(30);
            entity.Property(x => x.Score).HasPrecision(6, 5);
            entity.Property(x => x.Reason).HasMaxLength(500).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnType("datetime2(0)");

            entity.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.SourceProduct)
                .WithMany()
                .HasForeignKey(x => x.SourceProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.RecommendedProduct)
                .WithMany()
                .HasForeignKey(x => x.RecommendedProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
