using Microsoft.EntityFrameworkCore;
using Icms.Application.DTOs;
using Icms.Application.Interfaces;
using Icms.Domain.Entities;
using Icms.Domain.Enums;

namespace Icms.Infrastructure.Persistence;

public class TransferRepository(IcmsDbContext dbContext) : ITransferRepository
{
    public Task<Transfer?> GetByIdAsync(long id, CancellationToken ct) =>
        dbContext.Transfers.AsNoTracking()
            .Include(t => t.Member)
            .Include(t => t.SourceChurch)
            .Include(t => t.DestinationChurch)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public Task<Transfer?> GetTrackedByIdAsync(long id, CancellationToken ct) =>
        dbContext.Transfers
            .Include(t => t.Member)
            .Include(t => t.SourceChurch)
            .Include(t => t.DestinationChurch)
            .FirstOrDefaultAsync(t => t.Id == id, ct);

    public async Task<Transfer> AddAsync(Transfer transfer, CancellationToken ct)
    {
        dbContext.Transfers.Add(transfer);
        await dbContext.SaveChangesAsync(ct);
        return transfer;
    }

    public async Task<Transfer> UpdateAsync(Transfer transfer, CancellationToken ct)
    {
        dbContext.Transfers.Update(transfer);
        await dbContext.SaveChangesAsync(ct);
        return transfer;
    }

    public async Task<TransferSnapshot> AddSnapshotAsync(TransferSnapshot snapshot, CancellationToken ct)
    {
        dbContext.TransferSnapshots.Add(snapshot);
        await dbContext.SaveChangesAsync(ct);
        return snapshot;
    }

    public Task<List<Transfer>> GetPagedAsync(
        long? sourceChurchId,
        long? destinationChurchId,
        TransferDirection? direction,
        TransferStatus? status,
        string? search,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        return BuildQuery(sourceChurchId, destinationChurchId, direction, status, search)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(t => t.Member)
            .Include(t => t.SourceChurch)
            .Include(t => t.DestinationChurch)
            .ToListAsync(ct);
    }

    public Task<int> CountAsync(
        long? sourceChurchId,
        long? destinationChurchId,
        TransferDirection? direction,
        TransferStatus? status,
        string? search,
        CancellationToken ct)
    {
        return BuildQuery(sourceChurchId, destinationChurchId, direction, status, search).CountAsync(ct);
    }

    public async Task<TransferStatsDto> GetStatsAsync(long? churchId, long? districtId, CancellationToken ct)
    {
        var query = dbContext.Transfers.AsNoTracking().Where(t => !t.IsDeleted);

        if (districtId.HasValue)
        {
            var churchIds = await dbContext.Churches
                .Where(c => c.DistrictId == districtId.Value && !c.IsDeleted)
                .Select(c => c.Id)
                .ToListAsync(ct);
                
            query = query.Where(t => 
                (t.SourceChurchId.HasValue && churchIds.Contains(t.SourceChurchId.Value)) ||
                (t.DestinationChurchId.HasValue && churchIds.Contains(t.DestinationChurchId.Value)));
        }
        else if (churchId.HasValue)
        {
            query = query.Where(t => t.SourceChurchId == churchId.Value || t.DestinationChurchId == churchId.Value);
        }

        return new TransferStatsDto
        {
            Total = await query.CountAsync(ct),
            Initiated = await query.CountAsync(t => t.Status == TransferStatus.Initiated, ct),
            Completed = await query.CountAsync(t => t.Status == TransferStatus.Completed, ct),
            Voided = await query.CountAsync(t => t.Status == TransferStatus.Voided, ct),
            Incoming = await query.CountAsync(t => churchId.HasValue ? t.DestinationChurchId == churchId.Value : t.Direction == TransferDirection.Incoming, ct),
            Outgoing = await query.CountAsync(t => churchId.HasValue ? (t.SourceChurchId == churchId.Value && t.Type != TransferType.Rejoin) : t.Direction == TransferDirection.Outgoing, ct),
            Internal = await query.CountAsync(t => t.Type == TransferType.Internal, ct),
            External = await query.CountAsync(t => t.Type != TransferType.Internal, ct)
        };
    }

    private IQueryable<Transfer> BuildQuery(
        long? sourceChurchId,
        long? destinationChurchId,
        TransferDirection? direction,
        TransferStatus? status,
        string? search)
    {
        var query = dbContext.Transfers.AsNoTracking().Where(t => !t.IsDeleted);

        if (sourceChurchId.HasValue && destinationChurchId.HasValue)
        {
            query = query.Where(t => (t.SourceChurchId == sourceChurchId.Value && t.Type != TransferType.Rejoin) || t.DestinationChurchId == destinationChurchId.Value);
        }
        else if (sourceChurchId.HasValue)
        {
            query = query.Where(t => t.SourceChurchId == sourceChurchId.Value && t.Type != TransferType.Rejoin);
        }
        else if (destinationChurchId.HasValue)
        {
            query = query.Where(t => t.DestinationChurchId == destinationChurchId.Value);
        }

        if (direction.HasValue)
            query = query.Where(t => t.Direction == direction.Value);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search}%";
            query = query.Where(t =>
                EF.Functions.ILike(t.ClearanceCode, term) ||
                (t.Member != null && EF.Functions.ILike(t.Member.EfgbcId, term)) ||
                (t.Member != null && EF.Functions.ILike(t.Member.FirstName, term)) ||
                (t.DestinationChurchName != null && EF.Functions.ILike(t.DestinationChurchName, term)) ||
                (t.SourceChurchName != null && EF.Functions.ILike(t.SourceChurchName, term)));
        }

        return query;
    }
}
