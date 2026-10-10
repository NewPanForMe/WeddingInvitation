namespace WeddingInvitation.Domain.Enums;

/// <summary>宾客出席状态。</summary>
public enum GuestStatus
{
    /// <summary>待回复。</summary>
    Pending = 0,

    /// <summary>确认出席。</summary>
    Attending = 1,

    /// <summary>不出席。</summary>
    Declined = 2,
}
