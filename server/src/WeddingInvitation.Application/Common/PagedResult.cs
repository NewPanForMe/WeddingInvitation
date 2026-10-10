namespace WeddingInvitation.Application.Common;

/// <summary>分页结果。</summary>
public record PagedResult<T>(int Total, int Page, int PageSize, IReadOnlyList<T> Items);
