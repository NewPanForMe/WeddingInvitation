namespace WeddingInvitation.Application.Options;

/// <summary>背景音乐上传限制。</summary>
public class BackgroundMusicUploadOptions
{
    public const string SectionName = "BackgroundMusicUpload";

    /// <summary>单个背景音乐文件大小上限（MB）。</summary>
    public int MaxSizeMB { get; set; } = 15;
}
