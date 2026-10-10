using Microsoft.EntityFrameworkCore;
using WeddingInvitation.Application.Abstractions;
using WeddingInvitation.Application.Common;
using WeddingInvitation.Application.DTOs;
using WeddingInvitation.Domain.Entities;
using WeddingInvitation.Domain.Enums;

namespace WeddingInvitation.Application.Services;

/// <summary>出席回复（RSVP）提交。</summary>
public class RsvpService
{
    private readonly IApplicationDbContext _db;

    public RsvpService(IApplicationDbContext db) => _db = db;

    public async Task<RsvpResultDto> SubmitAsync(RsvpRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            throw ApiException.NotFound("邀请链接无效");
        }

        var guest = await _db.Guests.FirstOrDefaultAsync(g => g.Token == request.Token, cancellationToken)
            ?? throw ApiException.NotFound("邀请链接无效");

        var config = await _db.InvitationConfigs
            .AsNoTracking()
            .OrderBy(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (config?.RsvpDeadline is not null && config.RsvpDeadline.Value <= DateTimeOffset.UtcNow)
        {
            throw new ApiException("已超过回复截止时间", 400);
        }

        var guestCount = request.GuestCount;
        if (request.Attend)
        {
            if (guestCount < 1)
            {
                guestCount = 1;
            }
        }
        else
        {
            guestCount = 0;
        }

        var now = DateTimeOffset.UtcNow;
        var rsvp = await _db.Rsvps.FirstOrDefaultAsync(r => r.GuestId == guest.Id, cancellationToken);
        if (rsvp is null)
        {
            rsvp = new Rsvp { GuestId = guest.Id, CreatedAt = now };
            _db.Rsvps.Add(rsvp);
        }

        rsvp.Attend = request.Attend;
        rsvp.GuestCount = guestCount;
        rsvp.Remark = string.IsNullOrWhiteSpace(request.Remark) ? null : request.Remark.Trim();
        rsvp.UpdatedAt = now;

        guest.Status = request.Attend ? GuestStatus.Attending : GuestStatus.Declined;
        guest.UpdatedAt = now;

        await _db.SaveChangesAsync(cancellationToken);

        var status = Mapping.StatusToText(guest.Status);
        return new RsvpResultDto(true, request.Attend ? "回复成功，期待您的到来" : "回复成功，感谢您的告知", status);
    }
}
