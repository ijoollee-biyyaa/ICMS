using Icms.Application.Common;
using Icms.Application.DTOs;

namespace Icms.Application.Interfaces;

public interface IDistrictService
{
    Task<Result<DistrictDetailDto, DistrictError>> CreateDistrictAsync(
        CreateDistrictRequest request, CancellationToken ct);

    Task<Result<DistrictDetailDto, DistrictError>> GetDistrictAsync(long id, CancellationToken ct);

    Task<Result<DistrictDetailDto, DistrictError>> UpdateDistrictAsync(
        long id, UpdateDistrictRequest request, CancellationToken ct);

    Task<Result<DistrictDetailDto, DistrictError>> DeleteDistrictAsync(long id, CancellationToken ct);

    Task<PagedResponse<DistrictResponseDto>> GetDistrictsAsync(
        PagedRequest paging, CancellationToken ct);
}