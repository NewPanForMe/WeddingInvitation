namespace WeddingInvitation.Domain.Entities;

/// <summary>请柬配置（单行）。地址只保存详细地址，不使用经纬度。</summary>
public class InvitationConfig
{
    public long Id { get; set; }

    public string? GroomName { get; set; }

    public string? BrideName { get; set; }

    public DateTimeOffset? WeddingTime { get; set; }

    /// <summary>场地名称，例如“XX酒店 3楼宴会厅”。</summary>
    public string? Venue { get; set; }

    /// <summary>详细地址，例如“XX市XX区XX路88号”。</summary>
    public string? Address { get; set; }

    public string? CoverUrl { get; set; }

    public DateTimeOffset? RsvpDeadline { get; set; }

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
