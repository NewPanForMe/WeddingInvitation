using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeddingInvitation.Application.Common;
using WeddingInvitation.Application.Options;
using WeddingInvitation.Domain.Entities;
using WeddingInvitation.Domain.Enums;

namespace WeddingInvitation.Infrastructure.Persistence;

/// <summary>启动自检：确认数据库可连接，并在缺失时补齐请柬配置行与默认管理员。</summary>
public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");
        var db = provider.GetRequiredService<AppDbContext>();

        try
        {
            if (!await db.Database.CanConnectAsync(cancellationToken))
            {
                logger.LogError("无法连接 PostgreSQL，请检查取串接口/解密密钥或 PostgreSql:ConnectionString 配置。");
                return;
            }

            await EnsureSchemaAsync(db, cancellationToken);

            if (!await db.InvitationConfigs.AnyAsync(cancellationToken))
            {
                db.InvitationConfigs.Add(new InvitationConfig
                {
                    GroomName = "新郎",
                    BrideName = "新娘",
                    Venue = "酒店名称 宴会厅",
                    Address = "详细地址",
                    RsvpDeadline = DateTimeOffset.UtcNow.AddDays(7),
                    UpdatedAt = DateTimeOffset.UtcNow,
                });
                await db.SaveChangesAsync(cancellationToken);
                logger.LogInformation("已补种 invitation_config 初始数据（请在后台修改为真实信息）。");
            }

            var seed = provider.GetRequiredService<IOptions<AdminSeedOptions>>().Value;
            if (seed.Enabled && !await db.AdminUsers.AnyAsync(cancellationToken))
            {
                db.AdminUsers.Add(new AdminUser
                {
                    Username = string.IsNullOrWhiteSpace(seed.Username) ? "admin" : seed.Username.Trim(),
                    PasswordHash = PasswordHasher.Hash(string.IsNullOrWhiteSpace(seed.Password) ? "admin123" : seed.Password),
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                });
                await db.SaveChangesAsync(cancellationToken);
                logger.LogWarning("管理员表为空，已创建默认管理员 {Username}，请登录后立即修改密码。", seed.Username);
            }

            var guests = await db.Guests.CountAsync(cancellationToken);
            var photos = await db.Photos.CountAsync(cancellationToken);
            logger.LogInformation("数据库自检完成：宾客 {Guests} 位，照片 {Photos} 张。", guests, photos);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "数据库自检失败：{Message}", ex.Message);
        }
    }

    /// <summary>兼容功能上线前创建的数据库结构。</summary>
    private static async Task EnsureSchemaAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        await db.Database.ExecuteSqlRawAsync(
            "ALTER TABLE photos ADD COLUMN IF NOT EXISTS photo_type text NOT NULL DEFAULT 'moment';",
            cancellationToken);
        await db.Database.ExecuteSqlRawAsync(
            "ALTER TABLE invitation_config ADD COLUMN IF NOT EXISTS cover_show_in_moments boolean NOT NULL DEFAULT false;",
            cancellationToken);
        await db.Database.ExecuteSqlRawAsync(
            "ALTER TABLE invitation_config ADD COLUMN IF NOT EXISTS opening_message text NULL;",
            cancellationToken);
        await db.Database.ExecuteSqlRawAsync(
            "ALTER TABLE invitation_config ADD COLUMN IF NOT EXISTS story_title text NULL;",
            cancellationToken);
        await db.Database.ExecuteSqlRawAsync(
            "ALTER TABLE invitation_config ADD COLUMN IF NOT EXISTS story_content text NULL;",
            cancellationToken);
        await db.Database.ExecuteSqlRawAsync(
            "ALTER TABLE invitation_config ADD COLUMN IF NOT EXISTS closing_message text NULL;",
            cancellationToken);
        await db.Database.ExecuteSqlRawAsync(
            "ALTER TABLE invitation_config ADD COLUMN IF NOT EXISTS background_music_url text NULL;",
            cancellationToken);
        await db.Database.ExecuteSqlRawAsync(
            "ALTER TABLE invitation_config ADD COLUMN IF NOT EXISTS music_enabled boolean NOT NULL DEFAULT false;",
            cancellationToken);
        await db.Database.ExecuteSqlRawAsync(
            """
            UPDATE photos
            SET photo_type = 'cover'
            WHERE EXISTS (
                SELECT 1
                FROM invitation_config
                WHERE invitation_config.cover_url = photos.url
            );
            """,
            cancellationToken);
    }

    /// <summary>仅供开发调试：确保演示宾客存在。</summary>
    public static async Task EnsureDemoGuestAsync(AppDbContext db, CancellationToken cancellationToken = default)
    {
        const string token = "demo-token-0001";
        if (await db.Guests.AnyAsync(g => g.Token == token, cancellationToken))
        {
            return;
        }

        db.Guests.Add(new Guest
        {
            Name = "张三",
            Salutation = "亲爱的张三",
            Phone = "13800000000",
            Relation = "同学",
            TableNo = "A区 8号桌",
            MaxGuests = 2,
            Status = GuestStatus.Pending,
            Token = token,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync(cancellationToken);
    }
}
