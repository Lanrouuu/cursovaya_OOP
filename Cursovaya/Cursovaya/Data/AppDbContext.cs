using Cursovaya.Models;
using Microsoft.EntityFrameworkCore;

namespace Cursovaya.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Advertisement> Advertisements => Set<Advertisement>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<FavoriteAdvertisement> FavoriteAdvertisements => Set<FavoriteAdvertisement>();
    public DbSet<AppLog> AppLogs => Set<AppLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserName).IsRequired().HasMaxLength(80);
            entity.Property(x => x.Email).IsRequired().HasMaxLength(120);
            entity.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(30);
            entity.Property(x => x.PasswordHash).IsRequired().HasMaxLength(256);
            entity.Property(x => x.Role).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();

            entity.HasMany(x => x.Advertisements)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).IsRequired().HasMaxLength(80);
            entity.Property(x => x.Description).HasMaxLength(300);
            entity.HasIndex(x => x.Name).IsUnique();

            entity.HasMany(x => x.Advertisements)
                .WithOne(x => x.Category)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Advertisement>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).IsRequired().HasMaxLength(120);
            entity.Property(x => x.ShortDescription).IsRequired().HasMaxLength(250);
            entity.Property(x => x.FullDescription).IsRequired().HasMaxLength(2000);
            entity.Property(x => x.Price).HasPrecision(18, 2);
            entity.Property(x => x.City).IsRequired().HasMaxLength(80);
            entity.Property(x => x.ImagePath).HasMaxLength(400);
            entity.Property(x => x.SellerContactEmail).IsRequired().HasMaxLength(120);
            entity.Property(x => x.SellerContactPhone).IsRequired().HasMaxLength(30);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.Condition).IsRequired();
            entity.Property(x => x.Status).IsRequired();
        });

        modelBuilder.Entity<FavoriteAdvertisement>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasIndex(x => new { x.UserId, x.AdvertisementId }).IsUnique();

            entity.HasOne(x => x.User)
                .WithMany(x => x.FavoriteAdvertisements)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Advertisement)
                .WithMany(x => x.FavoriteAdvertisements)
                .HasForeignKey(x => x.AdvertisementId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AppLog>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Action).IsRequired().HasMaxLength(80);
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.CreatedAt).IsRequired();
        });
    }
}
