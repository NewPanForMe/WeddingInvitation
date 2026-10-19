using System.Reflection;
using WeddingInvitation.Domain.Entities;

var photoVisibilityType = typeof(Photo).Assembly.GetType("WeddingInvitation.Domain.Services.PhotoVisibility");
Assert(photoVisibilityType is not null, "PhotoVisibility must define which photos belong in 美好瞬间.");

var shouldShow = photoVisibilityType!.GetMethod(
    "ShouldShowInMoments",
    BindingFlags.Public | BindingFlags.Static);
Assert(shouldShow is not null, "PhotoVisibility.ShouldShowInMoments must be public and static.");

var moment = CreatePhoto("/uploads/moment.jpg", "Moment", true);
var cover = CreatePhoto("/uploads/cover.jpg", "Cover", true);
var legacyCover = CreatePhoto("/uploads/cover.jpg", "Moment", true);

Assert(Invoke(shouldShow!, moment, "/uploads/cover.jpg"), "An enabled moment photo should be shown.");
Assert(!Invoke(shouldShow!, cover, "/uploads/cover.jpg"), "A cover photo must not be shown as a moment.");
Assert(!Invoke(shouldShow!, legacyCover, "/uploads/cover.jpg"), "The current cover URL must be excluded from moment photos.");

var uploadPolicyType = typeof(Photo).Assembly.GetType("WeddingInvitation.Application.Common.PhotoUploadPolicy");
Assert(uploadPolicyType is not null, "PhotoUploadPolicy must define type-specific upload limits.");
var exceedsMaximumSize = uploadPolicyType!.GetMethod(
    "ExceedsMaximumSize",
    BindingFlags.Public | BindingFlags.Static);
Assert(exceedsMaximumSize is not null, "PhotoUploadPolicy.ExceedsMaximumSize must be public and static.");
Assert(
    !InvokeSize(exceedsMaximumSize!, "Cover", 25L * 1024 * 1024, 10),
    "A cover image must not be rejected by the ordinary photo size limit.");
Assert(
    InvokeSize(exceedsMaximumSize!, "Moment", 25L * 1024 * 1024, 10),
    "A moment image over the configured size limit must still be rejected.");

Console.WriteLine("PhotoVisibility rules passed.");

static Photo CreatePhoto(string url, string typeName, bool enabled)
{
    var photo = new Photo { Url = url, Enabled = enabled };
    var typeProperty = typeof(Photo).GetProperty("Type");
    Assert(typeProperty is not null && typeProperty.PropertyType.IsEnum, "Photo must expose an enum Type.");
    typeProperty!.SetValue(photo, Enum.Parse(typeProperty.PropertyType, typeName));
    return photo;
}

static bool Invoke(MethodInfo method, Photo photo, string coverUrl) =>
    method.Invoke(null, [photo, coverUrl]) is true;

static bool InvokeSize(MethodInfo method, string typeName, long contentLength, int maxSizeMb)
{
    var type = typeof(Photo).GetProperty("Type")!.PropertyType;
    return method.Invoke(null, [Enum.Parse(type, typeName), contentLength, maxSizeMb]) is true;
}

static void Assert(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}
