using Microsoft.EntityFrameworkCore;
using WeddingInvitation.Application.Abstractions;
using WeddingInvitation.Application.Common;
using WeddingInvitation.Application.DTOs;

namespace WeddingInvitation.Application.Services;

/// <summary>后台登录与口令管理。</summary>
public class AuthService
{
    private readonly IApplicationDbContext _db;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthService(IApplicationDbContext db, IJwtTokenGenerator tokenGenerator)
    {
        _db = db;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var username = request.Username?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(request.Password))
        {
            throw new ApiException("用户名或密码错误", 401);
        }

        var user = await _db.AdminUsers.FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new ApiException("用户名或密码错误", 401);
        }

        var (token, expiresAt) = _tokenGenerator.Generate(user);
        return new LoginResponse(token, expiresAt, user.Username);
    }

    public async Task<OperationResult> ChangePasswordAsync(long userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
        {
            throw new ApiException("新密码长度至少 6 位", 400);
        }

        var user = await _db.AdminUsers.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw ApiException.Unauthorized();

        if (!PasswordHasher.Verify(request.OldPassword ?? string.Empty, user.PasswordHash))
        {
            throw new ApiException("原密码不正确", 400);
        }

        user.PasswordHash = PasswordHasher.Hash(request.NewPassword);
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return new OperationResult(true, "密码修改成功");
    }
}
