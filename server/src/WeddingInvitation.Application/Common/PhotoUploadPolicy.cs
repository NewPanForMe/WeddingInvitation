using WeddingInvitation.Domain.Enums;

namespace WeddingInvitation.Application.Common;

/// <summary>按照片用途应用上传大小规则。</summary>
public static class PhotoUploadPolicy
{
    public static bool ExceedsMaximumSize(PhotoType type, long contentLength, int maxSizeMb) =>
        type != PhotoType.Cover && contentLength > Math.Max(1, maxSizeMb) * 1024L * 1024L;
}
