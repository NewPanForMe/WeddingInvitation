using WeddingInvitation.Application.DTOs;
using WeddingInvitation.Domain.Entities;
using WeddingInvitation.Domain.Enums;

namespace WeddingInvitation.Application.Services;

/// <summary>实体到 DTO 的映射（在内存中映射，避免 EF 投影的翻译差异）。</summary>
internal static class Mapping
{
    public static InvitationConfigDto ToConfigDto(InvitationConfig? config) => new(
        config?.GroomName,
        config?.BrideName,
        config?.WeddingTime,
        config?.Venue,
        config?.Address,
        config?.CoverUrl,
        config?.RsvpDeadline);

    public static PhotoDto ToPhotoDto(Photo photo) => new(
        photo.Id,
        photo.Url,
        photo.Title,
        photo.SortOrder,
        photo.Enabled,
        photo.CreatedAt);

    public static GuestDto ToGuestDto(Guest guest) => new(
        guest.Id,
        guest.Name,
        guest.Salutation,
        guest.Phone,
        guest.Relation,
        guest.TableNo,
        guest.MaxGuests,
        StatusToText(guest.Status),
        guest.Token,
        guest.Rsvp is not null,
        guest.Rsvp?.Attend,
        guest.Rsvp?.GuestCount,
        guest.Rsvp?.Remark,
        guest.Rsvp?.UpdatedAt,
        guest.CreatedAt);

    public static string StatusToText(GuestStatus status) => status switch
    {
        GuestStatus.Attending => "attending",
        GuestStatus.Declined => "declined",
        _ => "pending",
    };

    public static bool TryParseStatus(string? text, out GuestStatus status)
    {
        switch (text?.Trim().ToLowerInvariant())
        {
            case "attending":
                status = GuestStatus.Attending;
                return true;
            case "declined":
                status = GuestStatus.Declined;
                return true;
            case "pending":
                status = GuestStatus.Pending;
                return true;
            default:
                status = GuestStatus.Pending;
                return false;
        }
    }
}
