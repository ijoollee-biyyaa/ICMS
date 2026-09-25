using Icms.Application.Common;
using Icms.Application.DTOs;
using Icms.Domain.Enums;

namespace Icms.Application.Interfaces;

public interface ITransferService
{
    Task<Result<TransferResponseDto, TransferError>> InitiateOutgoingAsync(
        long sourceChurchId, InitiateTransferRequest request, string userId, CancellationToken ct);

    Task<Result<TransferResponseDto, TransferError>> AcceptIncomingAsync(
        long transferId, long destinationChurchId, AcceptTransferRequest request, string userId, CancellationToken ct);

    Task<Result<TransferResponseDto, TransferError>> RegisterExternalIncomingAsync(
        RegisterExternalIncomingRequest request, string userId, CancellationToken ct);

    Task<Result<TransferResponseDto, TransferError>> VoidTransferAsync(
        long transferId, long sourceChurchId, string reason, string userId, CancellationToken ct);

    Task<Result<TransferResponseDto, TransferError>> GetByIdAsync(
        long id, CancellationToken ct);

    Task<Result<PagedResponse<TransferResponseDto>, TransferError>> GetPagedAsync(
        long? sourceChurchId, long? destinationChurchId, TransferDirection? direction,
        TransferStatus? status, string? search, PagedRequest paging, CancellationToken ct);

    Task<Result<TransferStatsDto, TransferError>> GetStatsAsync(
        long? churchId, long? districtId, CancellationToken ct);

    Task<Result<byte[], TransferError>> GeneratePdfAsync(
        long id, CancellationToken ct);

    Task<Result<List<MemberLookupDto>, TransferError>> SearchRejoinCandidatesAsync(
        long churchId, string query, CancellationToken ct);

    Task<Result<TransferResponseDto, TransferError>> RejoinMemberAsync(
        long churchId, RejoinTransferRequest request, string userId, CancellationToken ct);
}
