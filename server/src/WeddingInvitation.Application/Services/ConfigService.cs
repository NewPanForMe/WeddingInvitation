using Microsoft.EntityFrameworkCore;
using WeddingInvitation.Application.Abstractions;
using WeddingInvitation.Application.DTOs;
using WeddingInvitation.Domain.Entities;

namespace WeddingInvitation.Application.Services;

/// <summary>请柬配置读写（单行）。</summary>
public class ConfigService
{
    private readonly IApplicationDbContext _db;

    public ConfigService(IApplicationDbContext db) => _db = db;

    public async Task<InvitationConfigDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var config = await _db.InvitationConfigs
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return Mapping.ToConfigDto(config);
    }

    public async Task<InvitationConfigDto> UpdateAsync(
        InvitationConfigUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var config = await _db.InvitationConfigs.OrderBy(c => c.Id).FirstOrDefaultAsync(cancellationToken);
        if (config is null)
        {
            config = new InvitationConfig();
            _db.InvitationConfigs.Add(config);
        }

        config.GroomName = Normalize(request.GroomName);
        config.BrideName = Normalize(request.BrideName);
        config.WeddingTime = ToUtc(request.WeddingTime);
        config.Venue = Normalize(request.Venue);
        config.Address = Normalize(request.Address);
        config.CoverUrl = Normalize(request.CoverUrl);
        config.RsvpDeadline = ToUtc(request.RsvpDeadline);
        config.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return Mapping.ToConfigDto(config);
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>Npgsql 的 timestamptz 只接受 UTC，统一归一化。</summary>
    private static DateTimeOffset? ToUtc(DateTimeOffset? value) =>
        value?.ToUniversalTime();
}
