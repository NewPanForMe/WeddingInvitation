using WeddingInvitation.Domain.Entities;
using WeddingInvitation.Domain.Enums;

namespace WeddingInvitation.Domain.Services;

/// <summary>统一判断照片是否属于“美好瞬间”。</summary>
public static class PhotoVisibility
{
    public static bool ShouldShowInMoments(Photo photo, string? coverUrl) =>
        photo.Enabled &&
        photo.Type == PhotoType.Moment &&
        !string.Equals(photo.Url, coverUrl, StringComparison.OrdinalIgnoreCase);
}
