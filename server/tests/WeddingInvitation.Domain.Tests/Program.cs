using System.Reflection;
using System.Text;
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

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
var csvTextEncodingType = typeof(Photo).Assembly.GetType("WeddingInvitation.Application.Common.CsvTextEncoding");
Assert(csvTextEncodingType is not null, "CsvTextEncoding must decode CSV files before import.");

var decodeCsv = csvTextEncodingType!.GetMethod("Decode", BindingFlags.Public | BindingFlags.Static);
Assert(decodeCsv is not null, "CsvTextEncoding.Decode must be public and static.");

var gb18030Csv = Encoding.GetEncoding("GB18030").GetBytes("姓名,称呼\r\n张三,李先生\r\n");
var decodedGb18030 = decodeCsv!.Invoke(null, [gb18030Csv]) as string;
Assert(decodedGb18030 == "姓名,称呼\r\n张三,李先生\r\n", "GB18030 CSV must preserve Chinese guest data.");

var atmospherePolicyType = typeof(Photo).Assembly.GetType("WeddingInvitation.Application.Common.InvitationAtmospherePolicy");
Assert(atmospherePolicyType is not null, "InvitationAtmospherePolicy must provide presentation rules for romantic invitation content.");

var normalizeText = atmospherePolicyType!.GetMethod("NormalizeText", BindingFlags.Public | BindingFlags.Static);
Assert(normalizeText is not null, "InvitationAtmospherePolicy.NormalizeText must be public and static.");
Assert(
    normalizeText!.Invoke(null, ["  与你携手，共赴余生  ", 60]) as string == "与你携手，共赴余生",
    "Invitation text must trim surrounding whitespace before it is shown to guests.");

var canShowMusic = atmospherePolicyType.GetMethod("CanShowBackgroundMusic", BindingFlags.Public | BindingFlags.Static);
Assert(canShowMusic is not null, "InvitationAtmospherePolicy.CanShowBackgroundMusic must be public and static.");
Assert(
    canShowMusic!.Invoke(null, [true, "/Wedding/upload/music/our-song.mp3"]) is true,
    "An enabled background-music URL must be available to guests.");
Assert(
    canShowMusic.Invoke(null, [false, "/Wedding/upload/music/our-song.mp3"]) is false,
    "Background music must remain hidden when the administrator turns it off.");

var backgroundMusicUploadPolicyType = typeof(Photo).Assembly.GetType("WeddingInvitation.Application.Common.BackgroundMusicUploadPolicy");
Assert(backgroundMusicUploadPolicyType is not null, "BackgroundMusicUploadPolicy must define allowed music files.");

var isAllowedMusicFile = backgroundMusicUploadPolicyType!.GetMethod("IsAllowedFileName", BindingFlags.Public | BindingFlags.Static);
Assert(isAllowedMusicFile is not null, "BackgroundMusicUploadPolicy.IsAllowedFileName must be public and static.");
Assert(isAllowedMusicFile!.Invoke(null, ["our-song.m4a"]) is true, "M4A music files must be accepted.");
Assert(isAllowedMusicFile.Invoke(null, ["our-song.wav"]) is false, "Unsupported music formats must be rejected.");

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
