using FluentValidation;
using Microsoft.Extensions.Logging;
using Icms.Application.Common;
using Icms.Application.DTOs;
using Icms.Application.Interfaces;
using Icms.Domain.Entities;
using Icms.Domain.Enums;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Icms.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Icms.Infrastructure.Identity;

namespace Icms.Infrastructure.Services;

public class TransferService(
    ITransferRepository transferRepository,
    IMemberRepository memberRepository,
    IChurchRepository churchRepository,
    IcmsDbContext dbContext,
    IEnumerable<IValidator<InitiateTransferRequest>> initiateValidators,
    IEnumerable<IValidator<RegisterExternalIncomingRequest>> externalValidators,
    UserManager<User> userManager,
    ILogger<TransferService> logger) : ITransferService
{
    public async Task<Result<TransferResponseDto, TransferError>> InitiateOutgoingAsync(
        long sourceChurchId, InitiateTransferRequest request, string userId, CancellationToken ct)
    {
        var validator = initiateValidators.FirstOrDefault();
        if (validator != null)
            await validator.ValidateAndThrowAsync(request, ct);

        var member = await memberRepository.GetByIdAsync(request.MemberId, ct);
        if (member == null)
            return Result<TransferResponseDto, TransferError>.Failure(TransferError.MemberNotFound(request.MemberId));

        if (member.ChurchId != sourceChurchId)
            return Result<TransferResponseDto, TransferError>.Failure(TransferError.InvalidState("Member does not belong to the source church"));

        if (member.Status != MemberStatus.Active)
            return Result<TransferResponseDto, TransferError>.Failure(TransferError.MemberNotActive(request.MemberId));

        var sourceChurch = await churchRepository.GetByIdAsync(sourceChurchId, ct);
        if (sourceChurch == null)
            return Result<TransferResponseDto, TransferError>.Failure(TransferError.ChurchNotFound(sourceChurchId));

        Church? destChurch = null;
        if (request.Type == TransferType.Internal && request.DestinationChurchId.HasValue)
        {
            destChurch = await churchRepository.GetByIdAsync(request.DestinationChurchId.Value, ct);
            if (destChurch == null)
                return Result<TransferResponseDto, TransferError>.Failure(TransferError.ChurchNotFound(request.DestinationChurchId.Value));
        }

        var transfer = new Transfer
        {
            MemberId = member.Id,
            SourceChurchId = sourceChurchId,
            SourceChurchName = sourceChurch.Name,
            Type = request.Type,
            Direction = TransferDirection.Outgoing,
            Status = TransferStatus.Initiated,
            ClearanceCode = GenerateClearanceCode(),
            RecommendationNotes = request.RecommendationNotes,
            InitiatedByUserId = userId,
            InitiatedAt = DateTimeOffset.UtcNow
        };

        if (request.Type == TransferType.Internal)
        {
            transfer.DestinationChurchId = destChurch?.Id;
            transfer.DestinationChurchName = destChurch?.Name;
            
            // Mark member as transferring
            member.Status = MemberStatus.Transferring;
        }
        else
        {
            transfer.DestinationChurchName = request.DestinationChurchName;
            transfer.DestinationDistrictOrDenomination = request.DestinationDistrictOrDenomination;
            // Cross-district and cross-denom are completed immediately upon generating clearance
            transfer.Status = TransferStatus.Completed;
            transfer.CompletedByUserId = userId;
            transfer.CompletedAt = DateTimeOffset.UtcNow;
            
            // Deactivate member permanently for external transfers
            member.Status = MemberStatus.Deactivated;
            member.DeactivationReason = DeactivationReason.ClearanceOut;
            
            // Lock their application account if they have one
            var user = await userManager.Users.FirstOrDefaultAsync(u => u.MemberId == member.Id, ct);
            if (user != null)
            {
                user.IsAccountLocked = true;
                user.LockReason = AccountLockReason.ClearanceOut;
                user.LockedAtUtc = DateTime.UtcNow;
                await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
                await userManager.UpdateAsync(user);
            }
        }

        var created = await transferRepository.AddAsync(transfer, ct);

        if (request.Type != TransferType.Internal)
        {
            // Clean up member for external exit AFTER transfer is saved so it has an ID
            await TakeSnapshotAndCleanMemberAsync(created, member, ct);
        }

        await memberRepository.UpdateAsync(member, ct);

        logger.LogInformation("Transfer {TransferId} initiated for Member {MemberId} to {Destination}", 
            created.Id, member.Id, created.DestinationChurchName);

        return Result<TransferResponseDto, TransferError>.Success(ToDto(created));
    }

    public async Task<Result<TransferResponseDto, TransferError>> AcceptIncomingAsync(
        long transferId, long destinationChurchId, AcceptTransferRequest request, string userId, CancellationToken ct)
    {
        var transfer = await transferRepository.GetTrackedByIdAsync(transferId, ct);
        if (transfer == null)
            return Result<TransferResponseDto, TransferError>.Failure(TransferError.NotFound(transferId));

        if (transfer.DestinationChurchId != destinationChurchId)
            return Result<TransferResponseDto, TransferError>.Failure(TransferError.InvalidState("Transfer is not directed to this church"));

        if (transfer.Status != TransferStatus.Initiated)
            return Result<TransferResponseDto, TransferError>.Failure(TransferError.InvalidState("Transfer is not in an initiated state"));

        if (transfer.MemberId == null)
            return Result<TransferResponseDto, TransferError>.Failure(TransferError.InvalidState("Transfer is missing member information"));

        var member = await memberRepository.GetByIdAsync(transfer.MemberId.Value, ct);
        if (member == null)
            return Result<TransferResponseDto, TransferError>.Failure(TransferError.MemberNotFound(transfer.MemberId.Value));

        // Take snapshot before modifying
        await TakeSnapshotAndCleanMemberAsync(transfer, member, ct);

        // Accept and assign to new church
        transfer.Status = TransferStatus.Completed;
        transfer.CompletedByUserId = userId;
        transfer.CompletedAt = DateTimeOffset.UtcNow;
        transfer.ClearanceDocumentUrl = request.ClearanceDocumentUrl;

        member.ChurchId = destinationChurchId;
        member.Status = MemberStatus.Active;
        await memberRepository.UpdateAsync(member, ct);

        var updated = await transferRepository.UpdateAsync(transfer, ct);

        logger.LogInformation("Transfer {TransferId} accepted for Member {MemberId} by Church {ChurchId}", 
            updated.Id, member.Id, destinationChurchId);

        return Result<TransferResponseDto, TransferError>.Success(ToDto(updated));
    }

    public async Task<Result<TransferResponseDto, TransferError>> RegisterExternalIncomingAsync(
        RegisterExternalIncomingRequest request, string userId, CancellationToken ct)
    {
        var validator = externalValidators.FirstOrDefault();
        if (validator != null)
            await validator.ValidateAndThrowAsync(request, ct);

        var destChurch = await churchRepository.GetByIdAsync(request.DestinationChurchId, ct);
        if (destChurch == null)
            return Result<TransferResponseDto, TransferError>.Failure(TransferError.ChurchNotFound(request.DestinationChurchId));

        var transfer = new Transfer
        {
            DestinationChurchId = destChurch.Id,
            DestinationChurchName = destChurch.Name,
            SourceChurchName = request.SourceChurchName,
            SourceDistrictOrDenomination = request.SourceDistrictOrDenomination,
            Type = TransferType.CrossDistrict, // Assume cross-district EFGBC or denom
            Direction = TransferDirection.Incoming,
            Status = TransferStatus.Completed,
            ClearanceCode = GenerateClearanceCode(),
            ClearanceDocumentUrl = request.ClearanceDocumentUrl,
            IncomingFirstName = request.IncomingFirstName,
            IncomingFatherName = request.IncomingFatherName,
            IncomingGrandfatherName = request.IncomingGrandfatherName,
            PreviousEfgbcId = request.PreviousEfgbcId,
            InitiatedByUserId = userId,
            InitiatedAt = DateTimeOffset.UtcNow,
            CompletedByUserId = userId,
            CompletedAt = DateTimeOffset.UtcNow
        };

        var created = await transferRepository.AddAsync(transfer, ct);

        logger.LogInformation("External incoming transfer {TransferId} registered at Church {ChurchId}", 
            created.Id, destChurch.Id);

        return Result<TransferResponseDto, TransferError>.Success(ToDto(created));
    }

    public async Task<Result<TransferResponseDto, TransferError>> VoidTransferAsync(
        long transferId, long sourceChurchId, string reason, string userId, CancellationToken ct)
    {
        var transfer = await transferRepository.GetTrackedByIdAsync(transferId, ct);
        if (transfer == null)
            return Result<TransferResponseDto, TransferError>.Failure(TransferError.NotFound(transferId));

        if (transfer.SourceChurchId != sourceChurchId)
            return Result<TransferResponseDto, TransferError>.Failure(TransferError.InvalidState("Transfer was not initiated by this church"));

        if (transfer.Status != TransferStatus.Initiated)
            return Result<TransferResponseDto, TransferError>.Failure(TransferError.InvalidState("Only initiated transfers can be voided"));

        transfer.Status = TransferStatus.Voided;
        transfer.VoidReason = reason;
        transfer.VoidedByUserId = userId;
        transfer.VoidedAt = DateTimeOffset.UtcNow;

        if (transfer.Type == TransferType.Internal && transfer.MemberId.HasValue)
        {
            var member = await memberRepository.GetByIdAsync(transfer.MemberId.Value, ct);
            if (member != null && member.Status == MemberStatus.Transferring)
            {
                member.Status = MemberStatus.Active;
                await memberRepository.UpdateAsync(member, ct);
            }
        }

        var updated = await transferRepository.UpdateAsync(transfer, ct);

        logger.LogInformation("Transfer {TransferId} voided", updated.Id);

        return Result<TransferResponseDto, TransferError>.Success(ToDto(updated));
    }

    public async Task<Result<TransferResponseDto, TransferError>> GetByIdAsync(long id, CancellationToken ct)
    {
        var transfer = await transferRepository.GetByIdAsync(id, ct);
        if (transfer == null)
            return Result<TransferResponseDto, TransferError>.Failure(TransferError.NotFound(id));

        return Result<TransferResponseDto, TransferError>.Success(ToDto(transfer));
    }

    public async Task<Result<PagedResponse<TransferResponseDto>, TransferError>> GetPagedAsync(
        long? sourceChurchId, long? destinationChurchId, TransferDirection? direction,
        TransferStatus? status, string? search, PagedRequest paging, CancellationToken ct)
    {
        var transfers = await transferRepository.GetPagedAsync(
            sourceChurchId, destinationChurchId, direction, status, search, paging.SafePage, paging.PageSize, ct);
            
        var count = await transferRepository.CountAsync(
            sourceChurchId, destinationChurchId, direction, status, search, ct);

        return Result<PagedResponse<TransferResponseDto>, TransferError>.Success(new PagedResponse<TransferResponseDto>
        {
            Items = transfers.Select(ToDto).ToList(),
            TotalCount = count,
            Page = paging.SafePage,
            PageSize = paging.PageSize
        });
    }

    public async Task<Result<TransferStatsDto, TransferError>> GetStatsAsync(
        long? churchId, long? districtId, CancellationToken ct)
    {
        var stats = await transferRepository.GetStatsAsync(churchId, districtId, ct);
        return Result<TransferStatsDto, TransferError>.Success(stats);
    }

    public Task<Result<byte[], TransferError>> GeneratePdfAsync(long id, CancellationToken ct)
    {
        // TODO: implement actual PDF generation
        return Task.FromResult(Result<byte[], TransferError>.Success(Array.Empty<byte>()));
    }

    public async Task<Result<List<MemberLookupDto>, TransferError>> SearchRejoinCandidatesAsync(
        long churchId, string query, CancellationToken ct)
    {
        var destChurch = await churchRepository.GetByIdAsync(churchId, ct);
        if (destChurch == null)
            return Result<List<MemberLookupDto>, TransferError>.Failure(TransferError.ChurchNotFound(churchId));

        var term = $"%{query}%";

        var candidates = await dbContext.Members.AsNoTracking()
            .Include(m => m.Church)
            .Where(m => m.Church != null 
                        && m.Church.DistrictId == destChurch.DistrictId 
                        && m.Status == MemberStatus.Deactivated 
                        && m.DeactivationReason == DeactivationReason.ClearanceOut)
            .Where(m => EF.Functions.ILike(m.EfgbcId, term) 
                        || EF.Functions.ILike(m.FirstName, term)
                        || EF.Functions.ILike(m.FatherName, term)
                        || EF.Functions.ILike(m.GrandfatherName, term))
            .Take(20)
            .Select(m => new MemberLookupDto(
                m.Id, 
                m.EfgbcId, 
                MemberNames.Full(m), 
                m.Gender, 
                m.Church.Name))
            .ToListAsync(ct);

        return Result<List<MemberLookupDto>, TransferError>.Success(candidates);
    }

    public async Task<Result<TransferResponseDto, TransferError>> RejoinMemberAsync(
        long churchId, RejoinTransferRequest request, string userId, CancellationToken ct)
    {
        var destChurch = await churchRepository.GetByIdAsync(churchId, ct);
        if (destChurch == null)
            return Result<TransferResponseDto, TransferError>.Failure(TransferError.ChurchNotFound(churchId));

        var member = await memberRepository.GetByIdAsync(request.MemberId, ct);
        if (member == null)
            return Result<TransferResponseDto, TransferError>.Failure(TransferError.MemberNotFound(request.MemberId));

        if (member.Status != MemberStatus.Deactivated || member.DeactivationReason != DeactivationReason.ClearanceOut)
            return Result<TransferResponseDto, TransferError>.Failure(TransferError.InvalidState("Member is not eligible for rejoin."));

        var oldChurch = await churchRepository.GetByIdAsync(member.ChurchId, ct);

        var transfer = new Transfer
        {
            DestinationChurchId = destChurch.Id,
            DestinationChurchName = destChurch.Name,
            SourceChurchId = oldChurch?.Id,
            SourceChurchName = oldChurch?.Name,
            Type = TransferType.Rejoin,
            Direction = TransferDirection.Incoming,
            Status = TransferStatus.Completed,
            ClearanceCode = GenerateClearanceCode(),
            ClearanceDocumentUrl = request.ClearanceDocumentUrl,
            RecommendationNotes = request.RecommendationNotes,
            InitiatedByUserId = userId,
            InitiatedAt = DateTimeOffset.UtcNow,
            CompletedByUserId = userId,
            CompletedAt = DateTimeOffset.UtcNow,
            MemberId = member.Id
        };

        // Reactivate member
        member.ChurchId = destChurch.Id;
        member.Status = MemberStatus.Active;
        member.DeactivationReason = DeactivationReason.None;

        // Unlock account
        var user = await userManager.Users.FirstOrDefaultAsync(u => u.MemberId == member.Id, ct);
        if (user != null)
        {
            user.IsAccountLocked = false;
            user.LockReason = AccountLockReason.None;
            user.LockedAtUtc = null;
            await userManager.SetLockoutEndDateAsync(user, null);
            await userManager.UpdateAsync(user);
        }

        var created = await transferRepository.AddAsync(transfer, ct);
        await memberRepository.UpdateAsync(member, ct);

        logger.LogInformation("Transfer {TransferId} (Rejoin) completed for Member {MemberId} to {Destination}", 
            created.Id, member.Id, created.DestinationChurchName);

        return Result<TransferResponseDto, TransferError>.Success(ToDto(created));
    }

    private async Task TakeSnapshotAndCleanMemberAsync(Transfer transfer, Member member, CancellationToken ct)
    {
        // Load collections to serialize
        await dbContext.Entry(member).Collection(m => m.TeamMemberships).LoadAsync(ct);
        await dbContext.Entry(member).Collection(m => m.Employees).LoadAsync(ct);
        
        var teamsJson = JsonSerializer.Serialize(member.TeamMemberships.Select(tm => new { tm.TeamId, tm.Role }));
        var deptsJson = JsonSerializer.Serialize(member.Employees.Select(e => new { e.Id, e.Position }));

        var snapshot = new TransferSnapshot
        {
            TransferId = transfer.Id,
            MemberId = member.Id,
            TeamsJson = teamsJson,
            DepartmentsJson = deptsJson,
            SnapshotAt = DateTimeOffset.UtcNow
        };
        
        await transferRepository.AddSnapshotAsync(snapshot, ct);

        // Clean up
        dbContext.TeamMembers.RemoveRange(member.TeamMemberships);
        // Assuming we keep employee record but mark it deleted or handled via EmployeeService
        foreach (var emp in member.Employees.Where(e => e.ChurchId == member.ChurchId))
        {
            emp.IsDeleted = true;
        }
    }

    private static string GenerateClearanceCode()
    {
        return $"CLR-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
    }

    private static TransferResponseDto ToDto(Transfer t) => new()
    {
        Id = t.Id,
        MemberId = t.MemberId,
        MemberName = t.Member != null ? MemberNames.Full(t.Member) : null,
        MemberEfgbcId = t.Member?.EfgbcId,
        SourceChurchId = t.SourceChurchId,
        SourceChurchName = t.SourceChurchName,
        SourceDistrictOrDenomination = t.SourceDistrictOrDenomination,
        DestinationChurchId = t.DestinationChurchId,
        DestinationChurchName = t.DestinationChurchName,
        DestinationDistrictOrDenomination = t.DestinationDistrictOrDenomination,
        Type = t.Type,
        Direction = t.Direction,
        Status = t.Status,
        ClearanceCode = t.ClearanceCode,
        ClearanceDocumentUrl = t.ClearanceDocumentUrl,
        RecommendationNotes = t.RecommendationNotes,
        IncomingFirstName = t.IncomingFirstName,
        IncomingFatherName = t.IncomingFatherName,
        IncomingGrandfatherName = t.IncomingGrandfatherName,
        PreviousEfgbcId = t.PreviousEfgbcId,
        InitiatedByUserId = t.InitiatedByUserId,
        InitiatedAt = t.InitiatedAt,
        CompletedByUserId = t.CompletedByUserId,
        CompletedAt = t.CompletedAt,
        VoidReason = t.VoidReason,
        VoidedAt = t.VoidedAt,
        CreatedAt = t.CreatedAt
    };
}
