namespace WeddingInvitation.Domain.Entities;

/// <summary>出席回复（一名宾客一条，可修改）。</summary>
public class Rsvp
{
    public long Id { get; set; }

    public long GuestId { get; set; }

    public bool? Attend { get; set; }

    public int GuestCount { get; set; } = 1;

    public string? Remark { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Guest? Guest { get; set; }
}
