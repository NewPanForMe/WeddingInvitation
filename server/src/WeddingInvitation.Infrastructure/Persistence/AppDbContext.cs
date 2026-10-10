using Microsoft.EntityFrameworkCore;
using WeddingInvitation.Application.Abstractions;
using WeddingInvitation.Domain.Entities;
using WeddingInvitation.Domain.Enums;

namespace WeddingInvitation.Infrastructure.Persistence;

/// <summary>
/// PostgreSQL 数据上下文。表名/列名统一使用 snake_case，与 db/001_init.sql 一一对应。
/// </summary>
public class AppDbContext : DbContext, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();

    public DbSet<Guest> Guests => Set<Guest>();

    public DbSet<Photo> Photos => Set<Photo>();

    public DbSet<InvitationConfig> InvitationConfigs => Set<InvitationConfig>();

    public DbSet<Rsvp> Rsvps => Set<Rsvp>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AdminUser>(builder =>
        {
            builder.ToTable("admin_users");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Username).IsRequired();
            builder.Property(x => x.PasswordHash).IsRequired();
            builder.HasIndex(x => x.Username).IsUnique().HasDatabaseName("ux_admin_users_username");
        });

        modelBuilder.Entity<Guest>(builder =>
        {
            builder.ToTable("guests");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired();
            builder.Property(x => x.Token).IsRequired();
            builder.Property(x => x.Status).HasConversion(
                v => v == GuestStatus.Attending ? "attending" : v == GuestStatus.Declined ? "declined" : "pending",
                v => v == "attending" ? GuestStatus.Attending : v == "declined" ? GuestStatus.Declined : GuestStatus.Pending);
            builder.HasIndex(x => x.Token).IsUnique().HasDatabaseName("idx_guests_token");
            builder.HasIndex(x => x.Status).HasDatabaseName("idx_guests_status");
        });

        modelBuilder.Entity<Photo>(builder =>
        {
            builder.ToTable("photos");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Url).IsRequired();
            builder.HasIndex(x => new { x.SortOrder, x.Id }).HasDatabaseName("idx_photos_sort");
        });

        modelBuilder.Entity<InvitationConfig>(builder =>
        {
            builder.ToTable("invitation_config");
            builder.HasKey(x => x.Id);
        });

        modelBuilder.Entity<Rsvp>(builder =>
        {
            builder.ToTable("rsvp");
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => x.GuestId).IsUnique().HasDatabaseName("idx_rsvp_guest");
            builder.HasOne(x => x.Guest)
                .WithOne(g => g.Rsvp)
                .HasForeignKey<Rsvp>(x => x.GuestId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        ApplySnakeCaseNames(modelBuilder);
    }

    private static void ApplySnakeCaseNames(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            if (!string.IsNullOrEmpty(tableName))
            {
                entityType.SetTableName(SnakeCase.ToSnakeCase(tableName));
            }

            foreach (var property in entityType.GetProperties())
            {
                var columnName = property.GetColumnName();
                if (!string.IsNullOrEmpty(columnName))
                {
                    property.SetColumnName(SnakeCase.ToSnakeCase(columnName));
                }
            }

            foreach (var index in entityType.GetIndexes())
            {
                var indexName = index.GetDatabaseName();
                if (!string.IsNullOrEmpty(indexName))
                {
                    index.SetDatabaseName(SnakeCase.ToSnakeCase(indexName));
                }
            }
        }
    }

}
