using System.Text.Json;

namespace WeddingInvitation.Application.DTOs;

/// <summary>后台更新请柬配置。注意：地址只有详细地址，没有经纬度。</summary>
public record InvitationConfigUpdateRequest(
    string? GroomName,
    string? BrideName,
    DateTimeOffset? WeddingTime,
    string? Venue,
    string? Address,
    string? CoverUrl,
    bool CoverShowInMoments,
    string? OpeningMessage,
    string? StoryTitle,
    string? StoryContent,
    string? ClosingMessage,
    string? BackgroundMusicUrl,
    bool MusicEnabled,
    DateTimeOffset? RsvpDeadline);

/// <summary>背景音乐上传后的公开地址。</summary>
public record BackgroundMusicUploadDto(string Url);

/// <summary>后台更新照片信息。</summary>
public record PhotoUpdateRequest(string? Title, int SortOrder, bool Enabled)
{
    /// <summary>Undefined 表示旧客户端未传该字段；Null 表示显式清空。</summary>
    public JsonElement Description { get; init; }
}

/// <summary>后台批量重排照片顺序（按数组顺序赋 sortOrder）。</summary>
public record PhotoSortRequest(IReadOnlyList<long> Ids);
