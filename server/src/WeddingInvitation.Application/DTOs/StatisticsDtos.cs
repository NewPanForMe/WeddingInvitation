namespace WeddingInvitation.Application.DTOs;

/// <summary>后台统计。</summary>
public record StatisticsDto(
    int TotalGuests,
    int Responded,
    int Attending,
    int Declined,
    int Pending,
    int ExpectedAttendees,
    double ResponseRate);
