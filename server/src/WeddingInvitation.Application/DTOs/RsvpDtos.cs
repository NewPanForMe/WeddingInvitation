namespace WeddingInvitation.Application.DTOs;

/// <summary>POST /api/rsvp 请求。</summary>
public record RsvpRequest(string Token, bool Attend, int GuestCount, string? Remark);

public record RsvpResultDto(bool Success, string Message, string Status);
