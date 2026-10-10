using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WeddingInvitation.Application.Abstractions;
using WeddingInvitation.Application.Common;
using WeddingInvitation.Application.DTOs;
using WeddingInvitation.Application.Options;
using WeddingInvitation.Domain.Entities;
using WeddingInvitation.Domain.Enums;

namespace WeddingInvitation.Application.Services;

/// <summary>后台宾客管理。</summary>
public class GuestService
{
    private readonly IApplicationDbContext _db;
    private readonly InviteOptions _inviteOptions;

    public GuestService(IApplicationDbContext db, IOptions<InviteOptions> inviteOptions)
    {
        _db = db;
        _inviteOptions = inviteOptions.Value;
    }

    public async Task<PagedResult<GuestDto>> ListAsync(
        string? keyword,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 200 ? 20 : pageSize;

        var query = _db.Guests.AsNoTracking().Include(g => g.Rsvp).AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim();
            var lowered = k.ToLowerInvariant();
            query = query.Where(g =>
                g.Name.ToLower().Contains(lowered) ||
                (g.Phone != null && g.Phone.Contains(k)) ||
                (g.Relation != null && g.Relation.Contains(k)) ||
                (g.TableNo != null && g.TableNo.Contains(k)));
        }

        if (Mapping.TryParseStatus(status, out var parsedStatus))
        {
            query = query.Where(g => g.Status == parsedStatus);
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(g => g.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<GuestDto>(total, page, pageSize, items.Select(Mapping.ToGuestDto).ToList());
    }

    public async Task<GuestDto> CreateAsync(GuestUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var name = ValidateName(request.Name);
        var now = DateTimeOffset.UtcNow;
        var guest = new Guest
        {
            Name = name,
            Salutation = Normalize(request.Salutation),
            Phone = Normalize(request.Phone),
            Relation = Normalize(request.Relation),
            TableNo = Normalize(request.TableNo),
            MaxGuests = request.MaxGuests < 1 ? 1 : request.MaxGuests,
            Status = GuestStatus.Pending,
            Token = await GenerateUniqueTokenAsync(cancellationToken),
            CreatedAt = now,
            UpdatedAt = now,
        };

        _db.Guests.Add(guest);
        await _db.SaveChangesAsync(cancellationToken);
        return Mapping.ToGuestDto(guest);
    }

    public async Task<GuestDto> UpdateAsync(long id, GuestUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var guest = await _db.Guests.Include(g => g.Rsvp).FirstOrDefaultAsync(g => g.Id == id, cancellationToken)
            ?? throw ApiException.NotFound("宾客不存在");

        guest.Name = ValidateName(request.Name);
        guest.Salutation = Normalize(request.Salutation);
        guest.Phone = Normalize(request.Phone);
        guest.Relation = Normalize(request.Relation);
        guest.TableNo = Normalize(request.TableNo);
        guest.MaxGuests = request.MaxGuests < 1 ? 1 : request.MaxGuests;
        guest.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        return Mapping.ToGuestDto(guest);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var guest = await _db.Guests.FirstOrDefaultAsync(g => g.Id == id, cancellationToken)
            ?? throw ApiException.NotFound("宾客不存在");

        var rsvp = await _db.Rsvps.FirstOrDefaultAsync(r => r.GuestId == id, cancellationToken);
        if (rsvp is not null)
        {
            _db.Rsvps.Remove(rsvp);
        }

        _db.Guests.Remove(guest);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<GuestLinkDto> GetLinkAsync(long id, CancellationToken cancellationToken = default)
    {
        var guest = await _db.Guests.AsNoTracking().FirstOrDefaultAsync(g => g.Id == id, cancellationToken)
            ?? throw ApiException.NotFound("宾客不存在");

        return new GuestLinkDto(guest.Token, _inviteOptions.BuildUrl(guest.Token));
    }

    /// <summary>
    /// 从 CSV 文本批量导入：姓名,称呼,电话,关系,桌号,可带人数（表头行可省略）。
    /// </summary>
    public async Task<ImportResultDto> ImportAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        var text = await reader.ReadToEndAsync(cancellationToken);

        var errors = new List<string>();
        var imported = 0;
        var skipped = 0;
        var total = 0;
        var lineNo = 0;

        foreach (var rawLine in text.Split('\n'))
        {
            lineNo++;
            var line = rawLine.Trim('\r', ' ', '\t');
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var cells = SplitCsvLine(line);
            var first = cells.Count > 0 ? cells[0].Trim() : string.Empty;
            if (lineNo == 1 && (first.Contains("姓名", StringComparison.Ordinal) || first.Equals("name", StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            total++;
            if (string.IsNullOrWhiteSpace(first))
            {
                skipped++;
                errors.Add($"第{lineNo}行：姓名不能为空");
                continue;
            }

            var maxGuests = 1;
            if (cells.Count > 5 && int.TryParse(cells[5].Trim(), out var parsed) && parsed > 0)
            {
                maxGuests = parsed;
            }

            var now = DateTimeOffset.UtcNow;
            _db.Guests.Add(new Guest
            {
                Name = first.Length > 100 ? first[..100] : first,
                Salutation = Cell(cells, 1),
                Phone = Cell(cells, 2),
                Relation = Cell(cells, 3),
                TableNo = Cell(cells, 4),
                MaxGuests = maxGuests,
                Status = GuestStatus.Pending,
                Token = await GenerateUniqueTokenAsync(cancellationToken),
                CreatedAt = now,
                UpdatedAt = now,
            });
            imported++;
        }

        if (imported > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        return new ImportResultDto(total, imported, skipped, errors);
    }

    private static string? Cell(IReadOnlyList<string> cells, int index) =>
        cells.Count > index && !string.IsNullOrWhiteSpace(cells[index]) ? cells[index].Trim() : null;

    private static string ValidateName(string? name)
    {
        var value = name?.Trim();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ApiException("姓名不能为空", 400);
        }

        return value.Length > 100 ? value[..100] : value;
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private async Task<string> GenerateUniqueTokenAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)).ToLowerInvariant();
            if (!await _db.Guests.AnyAsync(g => g.Token == token, cancellationToken))
            {
                return token;
            }
        }

        throw new ApiException("生成宾客专属链接失败，请重试", 500);
    }

    /// <summary>简易 CSV 行解析：支持双引号包裹与 "" 转义。</summary>
    private static List<string> SplitCsvLine(string line)
    {
        var cells = new List<string>();
        var buffer = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        buffer.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    buffer.Append(c);
                }
            }
            else if (c == '"')
            {
                inQuotes = true;
            }
            else if (c == ',')
            {
                cells.Add(buffer.ToString());
                buffer.Clear();
            }
            else
            {
                buffer.Append(c);
            }
        }

        cells.Add(buffer.ToString());
        return cells;
    }
}
