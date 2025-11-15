using HOB.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HOB.Data;

public class HobDbContext : DbContext
{
    public HobDbContext(DbContextOptions<HobDbContext> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Sale> Sales => Set<Sale>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Customer entity
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.Property(e => e.Phone)
                .HasMaxLength(20);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            // Configure relationship
            entity.HasMany(e => e.Orders)
                .WithOne(e => e.Customer)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure Order entity
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId);

            entity.Property(e => e.OrderDate)
                .IsRequired();

            entity.Property(e => e.TotalAmount)
                .IsRequired()
                .HasPrecision(18, 2);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .IsRequired();

            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.OrderDate);
            entity.HasIndex(e => e.Status);

            // Configure relationship
            entity.HasMany(e => e.Sales)
                .WithOne(e => e.Order)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Sale entity
        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.SaleId);

            entity.Property(e => e.ProductName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Quantity)
                .IsRequired();

            entity.Property(e => e.UnitPrice)
                .IsRequired()
                .HasPrecision(18, 2);

            entity.Property(e => e.TotalPrice)
                .IsRequired()
                .HasPrecision(18, 2);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.HasIndex(e => e.OrderId);
            entity.HasIndex(e => e.ProductName);
        });

        // Seed data for development
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Customers
        var customer1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var customer2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");

        modelBuilder.Entity<Customer>().HasData(
            new Customer
            {
                CustomerId = customer1Id,
                Name = "John Doe",
                Email = "john.doe@example.com",
                Phone = "555-0100",
                CreatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc)
            },
            new Customer
            {
                CustomerId = customer2Id,
                Name = "Jane Smith",
                Email = "jane.smith@example.com",
                Phone = "555-0200",
                CreatedAt = new DateTime(2025, 1, 2, 10, 0, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 2, 10, 0, 0, DateTimeKind.Utc)
            }
        );

        // Seed Orders
        var order1Id = Guid.Parse("10000000-1000-1000-1000-100000000001");
        var order2Id = Guid.Parse("20000000-2000-2000-2000-200000000002");

        modelBuilder.Entity<Order>().HasData(
            new Order
            {
                OrderId = order1Id,
                CustomerId = customer1Id,
                OrderDate = new DateTime(2025, 1, 15, 14, 30, 0, DateTimeKind.Utc),
                TotalAmount = 150.00m,
                Status = "Completed",
                CreatedAt = new DateTime(2025, 1, 15, 14, 30, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 15, 14, 30, 0, DateTimeKind.Utc)
            },
            new Order
            {
                OrderId = order2Id,
                CustomerId = customer2Id,
                OrderDate = new DateTime(2025, 1, 16, 9, 15, 0, DateTimeKind.Utc),
                TotalAmount = 75.00m,
                Status = "Pending",
                CreatedAt = new DateTime(2025, 1, 16, 9, 15, 0, DateTimeKind.Utc),
                UpdatedAt = new DateTime(2025, 1, 16, 9, 15, 0, DateTimeKind.Utc)
            }
        );

        // Seed Sales
        modelBuilder.Entity<Sale>().HasData(
            new Sale
            {
                SaleId = Guid.Parse("a0000000-a000-a000-a000-a00000000001"),
                OrderId = order1Id,
                ProductName = "Widget A",
                Quantity = 2,
                UnitPrice = 50.00m,
                TotalPrice = 100.00m,
                CreatedAt = new DateTime(2025, 1, 15, 14, 30, 0, DateTimeKind.Utc)
            },
            new Sale
            {
                SaleId = Guid.Parse("a0000000-a000-a000-a000-a00000000002"),
                OrderId = order1Id,
                ProductName = "Widget B",
                Quantity = 1,
                UnitPrice = 50.00m,
                TotalPrice = 50.00m,
                CreatedAt = new DateTime(2025, 1, 15, 14, 30, 0, DateTimeKind.Utc)
            },
            new Sale
            {
                SaleId = Guid.Parse("a0000000-a000-a000-a000-a00000000003"),
                OrderId = order2Id,
                ProductName = "Widget C",
                Quantity = 1,
                UnitPrice = 75.00m,
                TotalPrice = 75.00m,
                CreatedAt = new DateTime(2025, 1, 16, 9, 15, 0, DateTimeKind.Utc)
            }
        );
    }
}
