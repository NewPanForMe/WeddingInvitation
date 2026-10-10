using Microsoft.EntityFrameworkCore;
using WeddingInvitation.Domain.Entities;

namespace WeddingInvitation.Application.Abstractions;

/// <summary>
/// 应用层使用的数据库上下文抽象，由 Infrastructure 的 AppDbContext 实现。
/// </summary>
public interface IApplicationDbContext
{
    DbSet<AdminUser> AdminUsers { get; }

    DbSet<Guest> Guests { get; }

    DbSet<Photo> Photos { get; }

    DbSet<InvitationConfig> InvitationConfigs { get; }

    DbSet<Rsvp> Rsvps { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
