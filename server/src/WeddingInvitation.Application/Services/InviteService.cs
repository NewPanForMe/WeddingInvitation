using Microsoft.EntityFrameworkCore;
using WeddingInvitation.Application.Abstractions;
using WeddingInvitation.Application.Common;
using WeddingInvitation.Application.DTOs;
using WeddingInvitation.Domain.Services;

namespace WeddingInvitation.Application.Services;

/// <summary>请柬公开接口：按 token 返回个性化请柬数据。</summary>
public class InviteService
{
    private readonly IApplicationDbContext _db;

    public InviteService(IApplicationDbContext db) => _db = db;

    public async Task<InviteResponse> GetAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw ApiException.NotFound("邀请链接无效");
        }

        var guest = await _db.Guests
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Token == token, cancellationToken)
            ?? throw ApiException.NotFound("邀请链接无效");

        var config = await _db.InvitationConfigs
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var photos = await _db.Photos
            .AsNoTracking()
            .Where(p => p.Enabled)
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Id)
            .ToListAsync(cancellationToken);

        var momentPhotos = photos
            .Where(photo => PhotoVisibility.ShouldShowInMoments(photo, config?.CoverUrl))
            .Select(Mapping.ToPhotoDto)
            .ToList();

        if (config?.CoverShowInMoments == true && !string.IsNullOrWhiteSpace(config.CoverUrl))
        {
            momentPhotos.Insert(0, new PhotoDto(
                0,
                config.CoverUrl,
                "婚礼封面",
                0,
                true,
                "cover",
                config.UpdatedAt));
        }

        var rsvp = await _db.Rsvps
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.GuestId == guest.Id, cancellationToken);

        var rsvpOpen = config?.RsvpDeadline is null || config.RsvpDeadline.Value > DateTimeOffset.UtcNow;

        return new InviteResponse(
            new GuestInfoDto(guest.Name, guest.Salutation, guest.TableNo, guest.MaxGuests),
            Mapping.ToConfigDto(config),
            momentPhotos,
            new RsvpInfoDto(rsvp is not null, rsvp?.Attend, rsvp?.GuestCount, rsvp?.Remark, rsvp?.UpdatedAt),
            rsvpOpen);
    }
}
