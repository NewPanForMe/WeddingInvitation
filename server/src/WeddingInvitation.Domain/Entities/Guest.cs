using WeddingInvitation.Domain.Enums;

namespace WeddingInvitation.Domain.Entities;

/// <summary>被邀请的宾客，token 为其专属请柬链接凭证。</summary>
public class Guest
{
    public long Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>请柬上的称呼，例如“亲爱的张三”。</summary>
    public string? Salutation { get; set; }

    public string? Phone { get; set; }

    /// <summary>专属链接 token（32 位以上随机字符串）。</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>关系，例如“同学 / 同事 / 亲属”。</summary>
    public string? Relation { get; set; }

    /// <summary>桌号，例如“A区 8号桌”。</summary>
    public string? TableNo { get; set; }

    /// <summary>可携带人数（含本人）。</summary>
    public int MaxGuests { get; set; } = 1;

    public GuestStatus Status { get; set; } = GuestStatus.Pending;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Rsvp? Rsvp { get; set; }
}
