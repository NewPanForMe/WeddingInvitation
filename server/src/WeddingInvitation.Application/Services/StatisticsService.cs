using Microsoft.EntityFrameworkCore;
using WeddingInvitation.Application.Abstractions;
using WeddingInvitation.Application.DTOs;
using WeddingInvitation.Domain.Enums;

namespace WeddingInvitation.Application.Services;

/// <summary>后台统计。</summary>
public class StatisticsService
{
    private readonly IApplicationDbContext _db;

    public StatisticsService(IApplicationDbContext db) => _db = db;

    public async Task<StatisticsDto> GetAsync(CancellationToken cancellationToken = default)
    {
        var total = await _db.Guests.CountAsync(cancellationToken);
        var attended = await _db.Guests.CountAsync(g => g.Status == GuestStatus.Attending, cancellationToken);
        var declined = await _db.Guests.CountAsync(g => g.Status == GuestStatus.Declined, cancellationToken);
        var pending = await _db.Guests.CountAsync(g => g.Status == GuestStatus.Pending, cancellationToken);
        var responded = await _db.Rsvps.CountAsync(cancellationToken);
        var expectedAttendees = await _db.Rsvps
            .Where(r => r.Attend == true)
            .SumAsync(r => (int?)r.GuestCount, cancellationToken) ?? 0;

        var rate = total == 0 ? 0d : Math.Round((double)responded / total, 4);

        return new StatisticsDto(total, responded, attended, declined, pending, expectedAttendees, rate);
    }
}
