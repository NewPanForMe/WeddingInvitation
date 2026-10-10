using WeddingInvitation.Domain.Entities;

namespace WeddingInvitation.Application.Abstractions;

/// <summary>后台接口的 JWT 令牌生成器。</summary>
public interface IJwtTokenGenerator
{
    (string Token, DateTimeOffset ExpiresAt) Generate(AdminUser user);
}
