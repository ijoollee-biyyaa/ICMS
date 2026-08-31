using System.Security.Cryptography;

using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Icms.Application.Churches;
using Icms.Application.Common;
using Icms.Application.DTOs;
using Icms.Application.Exceptions;
using Icms.Application.Interfaces;
using Icms.Domain.Entities;
using Icms.Domain.Enums;
using Icms.Infrastructure.Identity;
using Icms.Infrastructure.Persistence;

namespace Icms.Infrastructure.Services;

public class ChurchService(
    IChurchRepository churchRepository,
    CreateChurchValidator createValidator,
    AddChurchAdminValidator addAdminValidator,
    UpdateChurchValidator updateValidator,
    UserManager<User> userManager,
    RoleManager<IdentityRole> roleManager,
    IcmsDbContext dbContext,
    ILogger<ChurchService> logger) : IChurchService
{
    public const string ChurchAdminRole = "ChurchAdmin";
    public const string ChurchAdminPosition = "Church Administrator";

    public async Task<Result<CreateChurchResponseDto, ChurchError>> CreateChurchAsync(
        long districtId, CreateChurchRequest request, CancellationToken ct)
    {
        await createValidator.ValidateAndThrowAsync(request, ct);

        if (!await churchRepository.DistrictExistsAsync(districtId, ct))
            return Result<CreateChurchResponseDto, ChurchError>.Failure(
                ChurchError.DistrictNotFound(districtId));

        if (await churchRepository.CodeExistsAsync(request.Code, ct))
            return Result<CreateChurchResponseDto, ChurchError>.Failure(
                ChurchError.CodeAlreadyExists(request.Code));

        var adminEmail = await GenerateUniqueAdminEmailAsync(request.Code, ct);
        if (await userManager.FindByEmailAsync(adminEmail) is not null)
            return Result<CreateChurchResponseDto, ChurchError>.Failure(
                ChurchError.AdminEmailAlreadyExists(adminEmail));

        if (request.Type == ChurchType.Daughter)
        {
            var parent = await churchRepository.GetByIdAsync(request.ParentChurchId!.Value, ct);
            if (parent is null)
                return Result<CreateChurchResponseDto, ChurchError>.Failure(
                    ChurchError.ParentNotFound(request.ParentChurchId.Value));

            if (parent.Type != ChurchType.Local)
                return Result<CreateChurchResponseDto, ChurchError>.Failure(
                    ChurchError.ParentNotLocal());
        }

        var church = new Church
        {
            DistrictId = districtId,
            Name = request.Name,
            Type = request.Type,
            Code = request.Code,
            ParentChurchId = request.ParentChurchId,
            City = request.City,
            Subcity = request.Subcity,
            Email = request.Email,
            Phone = request.Phone,
            Tel = request.Tel,
            MapAddress = request.MapAddress,
            WebsiteUrl = request.WebsiteUrl
        };

        try
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);

            var created = await churchRepository.AddAsync(church, ct);

            var tempPassword = GenerateTempPassword();
            var adminResult = await CreateChurchAdminAccountAsync(created.Id,
                request.AdminFirstName, request.AdminFatherName, request.AdminGrandfatherName,
                adminEmail, tempPassword, ct);

            if (adminResult is not null)
            {
                await transaction.RollbackAsync(ct);
                return Result<CreateChurchResponseDto, ChurchError>.Failure(adminResult);
            }

            await transaction.CommitAsync(ct);

            logger.LogInformation("Created church {ChurchId} ({Code}) with admin account",
                created.Id, created.Code);

            return Result<CreateChurchResponseDto, ChurchError>.Success(
                new CreateChurchResponseDto(ToDto(created), adminEmail, tempPassword));
        }
        catch (UniqueConstraintViolationException)
        {
            return Result<CreateChurchResponseDto, ChurchError>.Failure(
                ChurchError.CodeAlreadyExists(request.Code));
        }
    }

    public async Task<Result<CreateChurchResponseDto, ChurchError>> AddChurchAdminAsync(
        long churchId, AddChurchAdminRequest request, CancellationToken ct)
    {
        await addAdminValidator.ValidateAndThrowAsync(request, ct);

        var church = await churchRepository.GetByIdAsync(churchId, ct);
        if (church is null)
            return Result<CreateChurchResponseDto, ChurchError>.Failure(ChurchError.NotFound(churchId));

        if (request.MemberId.HasValue)
        {
            return await AddExistingMemberAsAdminAsync(church, request.MemberId.Value, ct);
        }

        var adminEmail = await GenerateUniqueAdminEmailAsync(church.Code, ct);
        if (await userManager.FindByEmailAsync(adminEmail) is not null)
            return Result<CreateChurchResponseDto, ChurchError>.Failure(
                ChurchError.AdminEmailAlreadyExists(adminEmail));

        var tempPassword = GenerateTempPassword();
        var adminError = await CreateChurchAdminAccountAsync(
            churchId, request.FirstName!, request.FatherName!, request.GrandfatherName!,
            adminEmail, tempPassword, ct);
        if (adminError is not null)
            return Result<CreateChurchResponseDto, ChurchError>.Failure(adminError);

        logger.LogInformation("Added church admin for church {ChurchId}", churchId);

        return Result<CreateChurchResponseDto, ChurchError>.Success(
            new CreateChurchResponseDto(ToDto(church), adminEmail, tempPassword));
    }

    private async Task<Result<CreateChurchResponseDto, ChurchError>> AddExistingMemberAsAdminAsync(
        Church church, long memberId, CancellationToken ct)
    {
        var member = await dbContext.Members.AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == memberId && !m.IsDeleted, ct);

        if (member is null)
            return Result<CreateChurchResponseDto, ChurchError>.Failure(
                ChurchError.AdminMemberNotFound(memberId));

        if (member.ChurchId != church.Id)
            return Result<CreateChurchResponseDto, ChurchError>.Failure(
                ChurchError.AdminMemberNotInChurch());

        var linkedEmployee = await dbContext.Employees.AsNoTracking()
            .FirstOrDefaultAsync(e => e.MemberId == member.Id && e.UserId != null && !e.IsDeleted, ct);

        User? user = null;
        string? tempPassword = null;

        if (linkedEmployee is not null)
        {
            user = await userManager.FindByIdAsync(linkedEmployee.UserId!);
        }

        if (user is null)
        {
            var email = await GenerateUniqueAdminEmailAsync(church.Code, ct);
            tempPassword = GenerateTempPassword();

            user = new User
            {
                UserName = email,
                Email = email,
                FirstName = member.FirstName,
                FatherName = member.FatherName,
                GrandfatherName = member.GrandfatherName
            };

            var result = await userManager.CreateAsync(user, tempPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return Result<CreateChurchResponseDto, ChurchError>.Failure(
                    ChurchError.AdminCreationFailed(errors));
            }

            dbContext.Employees.Add(new Employee
            {
                ChurchId = church.Id,
                UserId = user.Id,
                MemberId = member.Id,
                EmploymentType = EmploymentType.ChurchStaff,
                Position = ChurchAdminPosition,
                HireDate = DateOnly.FromDateTime(DateTime.Today),
                SalaryPaidBy = SalaryPaidBy.Church,
                Status = EmployeeStatus.Active
            });

            await dbContext.SaveChangesAsync(ct);
        }

        if (!await roleManager.RoleExistsAsync(ChurchAdminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(ChurchAdminRole));
        }

        if (!await userManager.IsInRoleAsync(user, ChurchAdminRole))
        {
            await userManager.AddToRoleAsync(user, ChurchAdminRole);
        }

        logger.LogInformation("Promoted member {MemberId} to church admin of church {ChurchId}",
            member.Id, church.Id);

        return Result<CreateChurchResponseDto, ChurchError>.Success(
            new CreateChurchResponseDto(ToDto(church), user.Email!, tempPassword));
    }

    private async Task<ChurchError?> CreateChurchAdminAccountAsync(
        long churchId, string firstName, string fatherName, string grandfatherName,
        string email, string password, CancellationToken ct)
    {
        if (!await roleManager.RoleExistsAsync(ChurchAdminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(ChurchAdminRole));
        }

        var user = new User
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            FatherName = fatherName,
            GrandfatherName = grandfatherName
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            return ChurchError.AdminCreationFailed(errors);
        }

        await userManager.AddToRoleAsync(user, ChurchAdminRole);

        dbContext.Employees.Add(new Employee
        {
            ChurchId = churchId,
            UserId = user.Id,
            EmploymentType = EmploymentType.ChurchStaff,
            Position = ChurchAdminPosition,
            HireDate = DateOnly.FromDateTime(DateTime.Today),
            SalaryPaidBy = SalaryPaidBy.Church,
            Status = EmployeeStatus.Active
        });

        await dbContext.SaveChangesAsync(ct);

        return null;
    }

    private async Task<string> GenerateUniqueAdminEmailAsync(string churchCode, CancellationToken ct)
    {
        var baseEmail = $"admin.{churchCode.ToLowerInvariant()}@icms.et";

        if (await userManager.FindByEmailAsync(baseEmail) is null)
        {
            return baseEmail;
        }

        for (var suffix = 2; ; suffix++)
        {
            var candidate = $"admin.{churchCode.ToLowerInvariant()}{suffix}@icms.et";
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
        Func<int, int> random = RandomNumberGenerator.GetInt32;

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

    public async Task<Result<ChurchDetailDto, ChurchError>> GetChurchDetailAsync(
        long id, CancellationToken ct)
    {
        var church = await churchRepository.GetByIdAsync(id, ct);
        if (church is null)
            return Result<ChurchDetailDto, ChurchError>.Failure(ChurchError.NotFound(id));

        var daughterCount = await churchRepository.CountDaughterChurchesAsync(id, ct);
        var memberCount = await churchRepository.CountMembersAsync(id, ct);

        var dto = new ChurchDetailDto(
            church.Id, church.Name, church.Type, church.Code, church.ParentChurchId,
            church.City, church.Subcity, church.Email, church.Phone, church.Tel,
            church.MapAddress, church.WebsiteUrl, daughterCount, memberCount);

        return Result<ChurchDetailDto, ChurchError>.Success(dto);
    }

    public async Task<Result<ChurchResponseDto, ChurchError>> GetChurchByIdAsync(
        long id, CancellationToken ct)
    {
        var church = await churchRepository.GetByIdAsync(id, ct);

        return church is null
            ? Result<ChurchResponseDto, ChurchError>.Failure(ChurchError.NotFound(id))
            : Result<ChurchResponseDto, ChurchError>.Success(ToDto(church));
    }

    public async Task<Result<ChurchResponseDto, ChurchError>> UpdateChurchAsync(
        long id, UpdateChurchRequest request, CancellationToken ct)
    {
        await updateValidator.ValidateAndThrowAsync(request, ct);

        var church = await churchRepository.GetByIdAsync(id, ct);
        if (church is null)
            return Result<ChurchResponseDto, ChurchError>.Failure(ChurchError.NotFound(id));

        if (request.Code != church.Code &&
            await churchRepository.CodeExistsAsync(request.Code, ct))
        {
            return Result<ChurchResponseDto, ChurchError>.Failure(
                ChurchError.CodeAlreadyExists(request.Code));
        }

        church.Name = request.Name;
        church.Code = request.Code;
        church.City = request.City;
        church.Subcity = request.Subcity;
        church.Email = request.Email;
        church.Phone = request.Phone;
        church.Tel = request.Tel;
        church.MapAddress = request.MapAddress;
        church.WebsiteUrl = request.WebsiteUrl;

        var updated = await churchRepository.UpdateAsync(church, ct);

        logger.LogInformation("Updated church {ChurchId} ({Code})", updated.Id, updated.Code);

        return Result<ChurchResponseDto, ChurchError>.Success(ToDto(updated));
    }

    public async Task<Result<ChurchResponseDto, ChurchError>> DeleteChurchAsync(
        long id, CancellationToken ct)
    {
        var church = await churchRepository.GetByIdAsync(id, ct);
        if (church is null)
            return Result<ChurchResponseDto, ChurchError>.Failure(ChurchError.NotFound(id));

        if (await churchRepository.HasDaughterChurchesAsync(id, ct))
            return Result<ChurchResponseDto, ChurchError>.Failure(
                ChurchError.CannotDeleteWithDaughters());

        if (await churchRepository.HasMembersAsync(id, ct))
            return Result<ChurchResponseDto, ChurchError>.Failure(
                ChurchError.CannotDeleteWithMembers());

        church.IsDeleted = true;

        var deleted = await churchRepository.UpdateAsync(church, ct);

        logger.LogInformation("Deleted church {ChurchId} ({Code})", deleted.Id, deleted.Code);

        return Result<ChurchResponseDto, ChurchError>.Success(ToDto(deleted));
    }

    public async Task<Result<PagedResponse<ChurchResponseDto>, ChurchError>> GetChurchesAsync(
        long districtId, PagedRequest paging, CancellationToken ct)
    {
        if (!await churchRepository.DistrictExistsAsync(districtId, ct))
            return Result<PagedResponse<ChurchResponseDto>, ChurchError>.Failure(
                ChurchError.DistrictNotFound(districtId));

        var page = paging.SafePage;
        var pageSize = paging.PageSize;

        var churches = await churchRepository.GetByDistrictPagedAsync(districtId, paging.Search, page, pageSize, ct);
        var totalCount = await churchRepository.CountByDistrictAsync(districtId, paging.Search, ct);

        return Result<PagedResponse<ChurchResponseDto>, ChurchError>.Success(
            new PagedResponse<ChurchResponseDto>
            {
                Items = churches.Select(ToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
    }

    private static ChurchResponseDto ToDto(Church church) =>
        new(church.Id, church.Name, church.Type, church.Code, church.ParentChurchId,
            church.City, church.Subcity, church.Email, church.Phone, church.Tel,
            church.MapAddress, church.WebsiteUrl);
}