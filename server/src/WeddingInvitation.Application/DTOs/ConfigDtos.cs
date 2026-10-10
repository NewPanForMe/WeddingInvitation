namespace WeddingInvitation.Application.DTOs;

/// <summary>后台更新请柬配置。注意：地址只有详细地址，没有经纬度。</summary>
public record InvitationConfigUpdateRequest(
    string? GroomName,
    string? BrideName,
    DateTimeOffset? WeddingTime,
    string? Venue,
    string? Address,
    string? CoverUrl,
    DateTimeOffset? RsvpDeadline);

/// <summary>后台更新照片信息。</summary>
public record PhotoUpdateRequest(string? Title, int SortOrder, bool Enabled);

/// <summary>后台批量重排照片顺序（按数组顺序赋 sortOrder）。</summary>
public record PhotoSortRequest(IReadOnlyList<long> Ids);
