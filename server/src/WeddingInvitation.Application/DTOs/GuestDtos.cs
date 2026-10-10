namespace WeddingInvitation.Application.DTOs;

/// <summary>后台新增/编辑宾客。</summary>
public record GuestUpsertRequest(
    string Name,
    string? Salutation,
    string? Phone,
    string? Relation,
    string? TableNo,
    int MaxGuests);

public record GuestDto(
    long Id,
    string Name,
    string? Salutation,
    string? Phone,
    string? Relation,
    string? TableNo,
    int MaxGuests,
    string Status,
    string Token,
    bool RsvpSubmitted,
    bool? Attend,
    int? GuestCount,
    string? Remark,
    DateTimeOffset? RsvpUpdatedAt,
    DateTimeOffset CreatedAt);

/// <summary>CSV 批量导入结果。</summary>
public record ImportResultDto(int Total, int Imported, int Skipped, IReadOnlyList<string> Errors);

/// <summary>宾客专属请柬链接。</summary>
public record GuestLinkDto(string Token, string Url);
