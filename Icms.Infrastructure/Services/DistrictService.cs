using FluentValidation;
using Microsoft.Extensions.Logging;
using Icms.Application.Common;
using Icms.Application.Districts;
using Icms.Application.DTOs;
using Icms.Application.Interfaces;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Services;

public class DistrictService(
    IDistrictRepository districtRepository,
    CreateDistrictValidator createValidator,
    UpdateDistrictValidator updateValidator,
    ILogger<DistrictService> logger) : IDistrictService
{
    public async Task<Result<DistrictDetailDto, DistrictError>> CreateDistrictAsync(
        CreateDistrictRequest request, CancellationToken ct)
    {
        await createValidator.ValidateAndThrowAsync(request, ct);

        var code = request.Code.Trim().ToUpperInvariant();

        if (await districtRepository.CodeExistsAsync(code, null, ct))
            return Result<DistrictDetailDto, DistrictError>.Failure(
                DistrictError.CodeAlreadyExists(code));

        var district = new District
        {
            Name = request.Name.Trim(),
            Code = code,
            Address = request.Address?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        var created = await districtRepository.AddAsync(district, ct);

        logger.LogInformation("Created district {DistrictId} ({Name}, {Code})",
            created.Id, created.Name, created.Code);

        return Result<DistrictDetailDto, DistrictError>.Success(ToDetailDto(created, 0, 0));
    }

    public async Task<Result<DistrictDetailDto, DistrictError>> GetDistrictAsync(
        long id, CancellationToken ct)
    {
        var district = await districtRepository.GetByIdAsync(id, ct);
        if (district is null)
            return Result<DistrictDetailDto, DistrictError>.Failure(DistrictError.NotFound(id));

        var churchCount = await districtRepository.CountChurchesAsync(id, ct);
        var memberCount = await districtRepository.CountMembersAsync(id, ct);

        return Result<DistrictDetailDto, DistrictError>.Success(
            ToDetailDto(district, churchCount, memberCount));
    }

    public async Task<Result<DistrictDetailDto, DistrictError>> UpdateDistrictAsync(
        long id, UpdateDistrictRequest request, CancellationToken ct)
    {
        await updateValidator.ValidateAndThrowAsync(request, ct);

        var district = await districtRepository.GetByIdAsync(id, ct);
        if (district is null)
            return Result<DistrictDetailDto, DistrictError>.Failure(DistrictError.NotFound(id));

        var code = request.Code.Trim().ToUpperInvariant();

        if (await districtRepository.CodeExistsAsync(code, id, ct))
            return Result<DistrictDetailDto, DistrictError>.Failure(
                DistrictError.CodeAlreadyExists(code));

        district.Name = request.Name.Trim();
        district.Code = code;
        district.Address = request.Address?.Trim();

        var updated = await districtRepository.UpdateAsync(district, ct);
        var churchCount = await districtRepository.CountChurchesAsync(id, ct);
        var memberCount = await districtRepository.CountMembersAsync(id, ct);

        logger.LogInformation("Updated district {DistrictId} ({Name})", updated.Id, updated.Name);

        return Result<DistrictDetailDto, DistrictError>.Success(
            ToDetailDto(updated, churchCount, memberCount));
    }

    public async Task<Result<DistrictDetailDto, DistrictError>> DeleteDistrictAsync(
        long id, CancellationToken ct)
    {
        var district = await districtRepository.GetByIdAsync(id, ct);
        if (district is null)
            return Result<DistrictDetailDto, DistrictError>.Failure(DistrictError.NotFound(id));

        if (await districtRepository.HasChurchesAsync(id, ct))
            return Result<DistrictDetailDto, DistrictError>.Failure(
                DistrictError.CannotDeleteWithChurches(id));

        district.IsDeleted = true;

        var deleted = await districtRepository.UpdateAsync(district, ct);

        logger.LogInformation("Deleted district {DistrictId} ({Name})", deleted.Id, deleted.Name);

        return Result<DistrictDetailDto, DistrictError>.Success(ToDetailDto(deleted, 0, 0));
    }

    public async Task<PagedResponse<DistrictResponseDto>> GetDistrictsAsync(
        PagedRequest paging, CancellationToken ct)
    {
        var page = paging.SafePage;

        var districts = await districtRepository.GetPagedAsync(
            paging.Search, page, paging.PageSize, ct);
        var totalCount = await districtRepository.CountAsync(paging.Search, ct);

        return new PagedResponse<DistrictResponseDto>
        {
            Items = districts.Select(d => ToDto(d)).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = paging.PageSize
        };
    }

    private static DistrictResponseDto ToDto(District d) =>
        new(d.Id, d.Name, d.Code, d.Address, d.CreatedAt);

    private static DistrictDetailDto ToDetailDto(District d, int churchCount, int memberCount) =>
        new(d.Id, d.Name, d.Code, d.Address, d.CreatedAt, churchCount, memberCount);
}