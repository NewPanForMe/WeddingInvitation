namespace WeddingInvitation.Application.DTOs;

/// <summary>请柬接口返回的宾客信息（不含手机号等隐私字段）。</summary>
public record GuestInfoDto(string Name, string? Salutation, string? TableNo, int MaxGuests);

/// <summary>请柬配置。地址只提供详细地址，不提供经纬度。</summary>
public record InvitationConfigDto(
    string? GroomName,
    string? BrideName,
    DateTimeOffset? WeddingTime,
    string? Venue,
    string? Address,
    string? CoverUrl,
    DateTimeOffset? RsvpDeadline);

public record PhotoDto(long Id, string Url, string? Title, int SortOrder, bool Enabled, DateTimeOffset CreatedAt);

public record RsvpInfoDto(bool Submitted, bool? Attend, int? GuestCount, string? Remark, DateTimeOffset? UpdatedAt);

/// <summary>GET /api/invite/{token} 响应。</summary>
public record InviteResponse(
    GuestInfoDto Guest,
    InvitationConfigDto Config,
    IReadOnlyList<PhotoDto> Photos,
    RsvpInfoDto Rsvp,
    bool RsvpOpen);
