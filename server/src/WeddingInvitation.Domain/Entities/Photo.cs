using WeddingInvitation.Domain.Enums;

namespace WeddingInvitation.Domain.Entities;

/// <summary>请柬照片墙中的一张照片（服务器本地目录存储）。</summary>
public class Photo
{
    public long Id { get; set; }

    /// <summary>访问路径，例如 /Wedding/upload/xxx.jpg。</summary>
    public string Url { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public bool Enabled { get; set; } = true;

    /// <summary>封面或美好瞬间照片。</summary>
    public PhotoType Type { get; set; } = PhotoType.Moment;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
