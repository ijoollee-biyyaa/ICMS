using Icms.Application.Common;
using Icms.Application.DTOs;
using Icms.Domain.Entities;
using Icms.Domain.Enums;

namespace Icms.Application.Interfaces;

public interface ITransferRepository
{
    Task<Transfer?> GetByIdAsync(long id, CancellationToken ct);
    Task<Transfer?> GetTrackedByIdAsync(long id, CancellationToken ct);
    Task<Transfer> AddAsync(Transfer transfer, CancellationToken ct);
    Task<Transfer> UpdateAsync(Transfer transfer, CancellationToken ct);
    Task<TransferSnapshot> AddSnapshotAsync(TransferSnapshot snapshot, CancellationToken ct);
    
    Task<List<Transfer>> GetPagedAsync(
        long? sourceChurchId,
        long? destinationChurchId,
        TransferDirection? direction,
        TransferStatus? status,
        string? search,
        int page,
        int pageSize,
        CancellationToken ct);
        
    Task<int> CountAsync(
        long? sourceChurchId,
        long? destinationChurchId,
        TransferDirection? direction,
        TransferStatus? status,
        string? search,
        CancellationToken ct);
        
    Task<TransferStatsDto> GetStatsAsync(long? churchId, long? districtId, CancellationToken ct);
}
