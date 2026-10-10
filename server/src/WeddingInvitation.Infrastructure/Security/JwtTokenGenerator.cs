using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WeddingInvitation.Application.Abstractions;
using WeddingInvitation.Application.Options;
using WeddingInvitation.Domain.Entities;

namespace WeddingInvitation.Infrastructure.Security;

/// <summary>基于对称密钥（HMAC-SHA256）的 JWT 生成器。</summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options) => _options = options.Value;

    public (string Token, DateTimeOffset ExpiresAt) Generate(AdminUser user)
    {
        if (string.IsNullOrWhiteSpace(_options.Key) || Encoding.UTF8.GetByteCount(_options.Key) < 32)
        {
            throw new InvalidOperationException("Security:Jwt:Key 未配置或长度不足 32 字节。");
        }

        var minutes = _options.ExpiresMinutes <= 0 ? 120 : _options.ExpiresMinutes;
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(minutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Role, "admin"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
