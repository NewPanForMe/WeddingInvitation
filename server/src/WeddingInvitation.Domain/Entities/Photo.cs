namespace WeddingInvitation.Domain.Entities;

/// <summary>请柬照片墙中的一张照片（服务器本地目录存储）。</summary>
public class Photo
{
    public long Id { get; set; }

    /// <summary>访问路径，例如 /uploads/xxx.jpg。</summary>
    public string Url { get; set; } = string.Empty;

    public string? Title { get; set; }

    public int SortOrder { get; set; }

    public bool Enabled { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
