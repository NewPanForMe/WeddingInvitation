namespace WeddingInvitation.Domain.Entities;

/// <summary>后台管理员（单管理员方案，仅一条记录）。</summary>
public class AdminUser
{
    public long Id { get; set; }

    public string Username { get; set; } = string.Empty;

    /// <summary>PBKDF2 哈希，格式 PBKDF2$迭代次数$salt(Base64)$hash(Base64)。</summary>
    public string PasswordHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
