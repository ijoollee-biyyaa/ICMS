using Icms.Application.Common;
using Icms.Application.DTOs;

namespace Icms.Application.Interfaces;

public interface IMemberService
{
    Task<Result<MemberResponseDto, MemberError>> RegisterMemberAsync(
        CreateMemberRequest request, CancellationToken ct);

    Task<Result<MemberResponseDto, MemberError>> GetMemberByIdAsync(
        long id, CancellationToken ct);

    Task<Result<MemberResponseDto, MemberError>> UpdateMemberAsync(
        long id, UpdateMemberRequest request, CancellationToken ct);

    Task<Result<MemberResponseDto, MemberError>> DeleteMemberAsync(
        long id, CancellationToken ct);

    Task<Result<PagedResponse<MemberResponseDto>, MemberError>> GetMembersAsync(
        long? churchId, PagedRequest paging, CancellationToken ct);

    Task<Result<MemberStatsDto, MemberError>> GetMemberStatsAsync(
        long? churchId, CancellationToken ct);

    Task<Result<MemberAccountInfoDto, MemberError>> GetMemberAccountAsync(
        long memberId, CancellationToken ct);

    Task<Result<IssueMemberCredentialsDto, MemberError>> IssueCredentialsAsync(
        long memberId, CancellationToken ct);
}