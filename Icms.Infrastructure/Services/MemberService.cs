using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Icms.Application.Common;
using Icms.Application.DTOs;
using Icms.Application.Exceptions;
using Icms.Application.Interfaces;
using Icms.Application.Members;
using Icms.Domain.Entities;
using Icms.Domain.Enums;
using Icms.Infrastructure.Identity;
using Icms.Infrastructure.Persistence;

namespace Icms.Infrastructure.Services;

public class MemberService(
    IMemberRepository memberRepository,
    IEfgbcIdGenerator efgbcIdGenerator,
    RegisterMemberValidator registerValidator,
    UpdateMemberValidator updateValidator,
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager,
    IcmsDbContext dbContext,
    ILogger<MemberService> logger) : IMemberService
{
    public async Task<Result<MemberResponseDto, MemberError>> RegisterMemberAsync(
        CreateMemberRequest request, CancellationToken ct)
    {
        await registerValidator.ValidateAndThrowAsync(request, ct);

        var church = await memberRepository.GetChurchAsync(request.ChurchId, ct);
        if (church is null)
            return Result<MemberResponseDto, MemberError>.Failure(
                MemberError.ChurchNotFound(request.ChurchId));

        var efgbcId = await efgbcIdGenerator.NextAsync(church.District.Code, ct);

        Transfer? clearanceTransfer = null;
        if (request.ClearanceId.HasValue)
        {
            clearanceTransfer = await dbContext.Transfers
                .FirstOrDefaultAsync(t => t.Id == request.ClearanceId.Value && t.DestinationChurchId == church.Id, ct);
        }

        var member = new Member
        {
            EfgbcId = efgbcId,
            ChurchId = church.Id,
            FirstName = request.FirstName,
            FatherName = request.FatherName,
            GrandfatherName = request.GrandfatherName,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            MaritalStatus = request.MaritalStatus,
            JobStatus = request.JobStatus,
            HealthStatus = request.HealthStatus,
            Phone = request.Phone,
            Email = request.Email,
            City = request.City,
            Subcity = request.Subcity,
            LocalAddress = request.LocalAddress,
            PhotoUrl = request.PhotoUrl,
            JoinedVia = clearanceTransfer != null ? JoinChannel.Transfer : request.JoinedVia,
            JoinedAt = request.JoinedAt ?? DateOnly.FromDateTime(DateTime.Today),
            ConversionDate = request.ConversionDate,
            BaptismPlace = request.BaptismPlace,
            BaptismDate = request.BaptismDate,
            SpiritualGift = request.SpiritualGift,
            ClearanceId = clearanceTransfer?.Id
        };

        try
        {
            var created = await memberRepository.AddAsync(member, ct);

            if (clearanceTransfer != null)
            {
                clearanceTransfer.MemberId = created.Id;
                clearanceTransfer.Status = TransferStatus.Completed;
                clearanceTransfer.CompletedAt ??= DateTimeOffset.UtcNow;
                await dbContext.SaveChangesAsync(ct);
            }

            logger.LogInformation(
                "Registered member {MemberId} ({EfgbcId}) at church {ChurchId}",
                created.Id, created.EfgbcId, created.ChurchId);

            return Result<MemberResponseDto, MemberError>.Success(ToDto(created));
        }
        catch (UniqueConstraintViolationException)
        {
            return Result<MemberResponseDto, MemberError>.Failure(
                MemberError.EfgbcIdAlreadyExists(efgbcId));
        }
    }

    public async Task<Result<MemberResponseDto, MemberError>> GetMemberByIdAsync(
        long id, CancellationToken ct)
    {
        var member = await memberRepository.GetByIdAsync(id, ct);
        if (member is null)
            return Result<MemberResponseDto, MemberError>.Failure(MemberError.NotFound(id));

        return Result<MemberResponseDto, MemberError>.Success(ToDto(member));
    }

    public async Task<Result<MemberResponseDto, MemberError>> UpdateMemberAsync(
        long id, UpdateMemberRequest request, CancellationToken ct)
    {
        await updateValidator.ValidateAndThrowAsync(request, ct);

        var member = await memberRepository.GetByIdAsync(id, ct);
        if (member is null)
            return Result<MemberResponseDto, MemberError>.Failure(MemberError.NotFound(id));

        member.FirstName = request.FirstName;
        member.FatherName = request.FatherName;
        member.GrandfatherName = request.GrandfatherName;
        member.DateOfBirth = request.DateOfBirth;
        member.Gender = request.Gender;
        member.MaritalStatus = request.MaritalStatus;
        member.JobStatus = request.JobStatus;
        member.HealthStatus = request.HealthStatus;
        member.Phone = request.Phone;
        member.Email = request.Email;
        member.City = request.City;
        member.Subcity = request.Subcity;
        member.LocalAddress = request.LocalAddress;
        if (request.PhotoUrl is not null)
        {
            member.PhotoUrl = string.IsNullOrWhiteSpace(request.PhotoUrl) ? null : request.PhotoUrl;
        }
        member.ConversionDate = request.ConversionDate;
        member.BaptismPlace = request.BaptismPlace;
        member.BaptismDate = request.BaptismDate;
        member.SpiritualGift = request.SpiritualGift;

        var updated = await memberRepository.UpdateAsync(member, ct);

        logger.LogInformation("Updated member {MemberId} ({EfgbcId})", updated.Id, updated.EfgbcId);

        return Result<MemberResponseDto, MemberError>.Success(ToDto(updated));
    }

    public async Task<Result<MemberResponseDto, MemberError>> UpdatePhotoUrlAsync(
        long id, string photoUrl, CancellationToken ct)
    {
        var member = await memberRepository.GetByIdAsync(id, ct);
        if (member is null)
            return Result<MemberResponseDto, MemberError>.Failure(MemberError.NotFound(id));

        member.PhotoUrl = string.IsNullOrWhiteSpace(photoUrl) ? null : photoUrl;
        var updated = await memberRepository.UpdateAsync(member, ct);

        logger.LogInformation("Updated photo for member {MemberId} ({EfgbcId})", updated.Id, updated.EfgbcId);

        return Result<MemberResponseDto, MemberError>.Success(ToDto(updated));
    }

    public async Task<Result<MemberResponseDto, MemberError>> DeleteMemberAsync(
        long id, CancellationToken ct)
    {
        var member = await memberRepository.GetByIdAsync(id, ct);
        if (member is null)
            return Result<MemberResponseDto, MemberError>.Failure(MemberError.NotFound(id));

        member.IsDeleted = true;

        var deleted = await memberRepository.UpdateAsync(member, ct);

        logger.LogInformation("Deleted member {MemberId} ({EfgbcId})", deleted.Id, deleted.EfgbcId);

        return Result<MemberResponseDto, MemberError>.Success(ToDto(deleted));
    }

    public async Task<Result<PagedResponse<MemberResponseDto>, MemberError>> GetMembersAsync(
        long? churchId, PagedRequest paging, CancellationToken ct)
    {
        if (churchId is not null)
        {
            var church = await memberRepository.GetChurchAsync(churchId.Value, ct);
            if (church is null)
                return Result<PagedResponse<MemberResponseDto>, MemberError>.Failure(
                    MemberError.ReferencedChurchNotFound(churchId.Value));
        }

        var page = paging.SafePage;
        var pageSize = paging.PageSize;

        var members = await memberRepository.GetPagedAsync(churchId, paging.Search, page, pageSize, ct);
        var totalCount = await memberRepository.CountAsync(churchId, paging.Search, ct);

        return Result<PagedResponse<MemberResponseDto>, MemberError>.Success(
            new PagedResponse<MemberResponseDto>
            {
                Items = members.Select(ToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
    }

    public async Task<Result<MemberStatsDto, MemberError>> GetMemberStatsAsync(
        long? churchId, CancellationToken ct)
    {
        if (churchId is not null)
        {
            var church = await memberRepository.GetChurchAsync(churchId.Value, ct);
            if (church is null)
                return Result<MemberStatsDto, MemberError>.Failure(
                    MemberError.ReferencedChurchNotFound(churchId.Value));
        }

        var query = dbContext.Members.AsNoTracking().Where(m => !m.IsDeleted);
        if (churchId is not null)
            query = query.Where(m => m.ChurchId == churchId.Value);

        var total = await query.CountAsync(ct);
        var active = await query.CountAsync(m => m.Status == MemberStatus.Active, ct);
        var transferring = await query.CountAsync(
            m => m.Status == MemberStatus.Transferring, ct);
        var inactive = await query.CountAsync(
            m => m.Status == MemberStatus.Deactivated || m.Status == MemberStatus.Archived, ct);

        return Result<MemberStatsDto, MemberError>.Success(
            new MemberStatsDto(total, active, transferring, inactive));
    }

    public async Task<Result<MemberAccountInfoDto, MemberError>> GetMemberAccountAsync(
        long memberId, CancellationToken ct)
    {
        var member = await memberRepository.GetByIdAsync(memberId, ct);
        if (member is null)
            return Result<MemberAccountInfoDto, MemberError>.Failure(
                MemberError.NotFound(memberId));

        var account = await FindMemberAccountAsync(memberId, ct);

        return Result<MemberAccountInfoDto, MemberError>.Success(
            new MemberAccountInfoDto(memberId, account is not null, account?.Email));
    }

    public async Task<Result<IssueMemberCredentialsDto, MemberError>> IssueCredentialsAsync(
        long memberId, CancellationToken ct)
    {
        var member = await memberRepository.GetByIdAsync(memberId, ct);
        if (member is null)
            return Result<IssueMemberCredentialsDto, MemberError>.Failure(
                MemberError.NotFound(memberId));

        var existing = await FindMemberAccountAsync(memberId, ct);
        if (existing is not null)
        {
            if (existing.MemberId is null)
            {
                existing.MemberId = memberId;
                await userManager.UpdateAsync(existing);
            }

            await EnsureMemberRoleAsync(existing);

            return Result<IssueMemberCredentialsDto, MemberError>.Success(
                new IssueMemberCredentialsDto(memberId, existing.Email!, null));
        }

        var email = await ResolveMemberEmailAsync(member, ct);
        var tempPassword = GenerateTempPassword();

        var user = new User
        {
            UserName = email,
            Email = email,
            FirstName = member.FirstName,
            FatherName = member.FatherName,
            GrandfatherName = member.GrandfatherName,
            MemberId = memberId
        };

        var create = await userManager.CreateAsync(user, tempPassword);
        if (!create.Succeeded)
        {
            var errors = string.Join("; ", create.Errors.Select(e => e.Description));
            return Result<IssueMemberCredentialsDto, MemberError>.Failure(
                MemberError.AccountCreationFailed(errors));
        }

        await EnsureMemberRoleAsync(user);

        logger.LogInformation(
            "Issued member login account {MemberId} ({EfgbcId}) as {Email}",
            memberId, member.EfgbcId, email);

        return Result<IssueMemberCredentialsDto, MemberError>.Success(
            new IssueMemberCredentialsDto(memberId, email, tempPassword));
    }

    private async Task<User?> FindMemberAccountAsync(long memberId, CancellationToken ct)
    {
        var direct = await dbContext.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.MemberId == memberId, ct);
        if (direct is not null)
            return direct;

        // Back-compat: accounts linked to a member through employment.
        var linkedUserId = await dbContext.Employees.AsNoTracking()
            .Where(e => e.MemberId == memberId && e.UserId != null && !e.IsDeleted)
            .Select(e => e.UserId)
            .FirstOrDefaultAsync(ct);

        return linkedUserId is null ? null : await userManager.FindByIdAsync(linkedUserId);
    }

    private async Task EnsureMemberRoleAsync(User user)
    {
        const string memberRole = "Member";
        if (!await roleManager.RoleExistsAsync(memberRole))
        {
            await roleManager.CreateAsync(new IdentityRole(memberRole));
        }

        if (!await userManager.IsInRoleAsync(user, memberRole))
        {
            await userManager.AddToRoleAsync(user, memberRole);
        }
    }

    private async Task<string> ResolveMemberEmailAsync(Member member, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(member.Email)
            && await userManager.FindByEmailAsync(member.Email.Trim()) is null)
        {
            return member.Email.Trim();
        }

        var baseEmail = $"member.{member.EfgbcId.ToLowerInvariant()}@icms.et";
        if (await userManager.FindByEmailAsync(baseEmail) is null)
        {
            return baseEmail;
        }

        for (var suffix = 2; ; suffix++)
        {
            var candidate = $"member.{member.EfgbcId.ToLowerInvariant()}{suffix}@icms.et";
            if (await userManager.FindByEmailAsync(candidate) is null)
            {
                return candidate;
            }
        }
    }

    private static string GenerateTempPassword()
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnpqrstuvwxyz";
        const string digits = "23456789";
        const string symbols = "!@#$%^&*";
        Func<int, int> random = System.Security.Cryptography.RandomNumberGenerator.GetInt32;

        var chars = new List<char>
        {
            upper[random(upper.Length)],
            lower[random(lower.Length)],
            digits[random(digits.Length)],
            symbols[random(symbols.Length)]
        };

        var pool = upper + lower + digits + symbols;
        for (var i = 4; i < 16; i++)
        {
            chars.Add(pool[random(pool.Length)]);
        }

        return new string(chars.OrderBy(_ => random(int.MaxValue)).ToArray());
    }

    private static MemberResponseDto ToDto(Member m) => new(
        m.Id, m.EfgbcId, m.ChurchId, m.FirstName, m.FatherName, m.GrandfatherName,
        m.DateOfBirth, m.Gender, m.MaritalStatus, m.JobStatus, m.HealthStatus,
        m.Phone, m.Email, m.City, m.Subcity, m.LocalAddress, m.PhotoUrl,
        m.Status, m.JoinedVia, m.JoinedAt, m.ConversionDate, m.BaptismPlace,
        m.BaptismDate, m.SpiritualGift, m.ClearanceId, m.CreatedAt);
}