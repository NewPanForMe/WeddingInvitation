namespace WeddingInvitation.Application.Options;

/// <summary>照片本地存储配置，对应 appsettings 的 Upload 节。</summary>
public class UploadOptions
{
    public const string SectionName = "Upload";

    /// <summary>物理根目录。相对路径会由 API 项目解析到项目根目录，例如 upload。</summary>
    public string RootPath { get; set; } = "upload";

    /// <summary>对外访问前缀，例如 /uploads。</summary>
    public string RequestPath { get; set; } = "/uploads";

    /// <summary>单文件大小上限（MB）。</summary>
    public int MaxSizeMB { get; set; } = 10;

    /// <summary>
    /// 允许的扩展名（含点，小写）。留空时由 PhotoService 使用默认集合。
    /// 注意：配置绑定对集合是"追加"语义，这里必须给空数组，否则会与 appsettings 中的配置项重复。
    /// </summary>
    public string[] AllowedExtensions { get; set; } = [];
}
