namespace WeddingInvitation.Application.DTOs;

public record LoginRequest(string Username, string Password);

public record LoginResponse(string Token, DateTimeOffset ExpiresAt, string Username);

public record ChangePasswordRequest(string OldPassword, string NewPassword);

public record OperationResult(bool Success, string Message);
