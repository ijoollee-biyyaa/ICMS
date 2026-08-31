using Icms.Application.Common;
using Icms.Application.DTOs;

namespace Icms.Application.Interfaces;

public interface IChurchService
{
    Task<Result<CreateChurchResponseDto, ChurchError>> CreateChurchAsync(
        long districtId, CreateChurchRequest request, CancellationToken ct);

    Task<Result<CreateChurchResponseDto, ChurchError>> AddChurchAdminAsync(
        long churchId, AddChurchAdminRequest request, CancellationToken ct);

    Task<Result<ChurchResponseDto, ChurchError>> GetChurchByIdAsync(
        long id, CancellationToken ct);

    Task<Result<ChurchDetailDto, ChurchError>> GetChurchDetailAsync(
        long id, CancellationToken ct);

    Task<Result<ChurchResponseDto, ChurchError>> UpdateChurchAsync(
        long id, UpdateChurchRequest request, CancellationToken ct);

    Task<Result<ChurchResponseDto, ChurchError>> DeleteChurchAsync(
        long id, CancellationToken ct);

    Task<Result<PagedResponse<ChurchResponseDto>, ChurchError>> GetChurchesAsync(
        long districtId, PagedRequest paging, CancellationToken ct);
}