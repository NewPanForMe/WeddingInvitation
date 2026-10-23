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

    /// <summary>是否将当前封面图同时展示在“美好瞬间”。</summary>
    public bool CoverShowInMoments { get; set; }

    /// <summary>封面上的简短寄语。</summary>
    public string? OpeningMessage { get; set; }

    /// <summary>恋爱故事标题。</summary>
    public string? StoryTitle { get; set; }

    /// <summary>恋爱故事正文。</summary>
    public string? StoryContent { get; set; }

    /// <summary>页面底部落款寄语。</summary>
    public string? ClosingMessage { get; set; }

    /// <summary>背景音乐公开访问地址。</summary>
    public string? BackgroundMusicUrl { get; set; }

    /// <summary>是否在请柬中提供背景音乐。</summary>
    public bool MusicEnabled { get; set; }

    public DateTimeOffset? RsvpDeadline { get; set; }

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
