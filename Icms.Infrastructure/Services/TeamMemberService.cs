using FluentValidation;
using Microsoft.Extensions.Logging;
using Icms.Application.Common;
using Icms.Application.DTOs;
using Icms.Application.Exceptions;
using Icms.Application.Interfaces;
using Icms.Application.Teams;
using Icms.Domain.Entities;
using Icms.Domain.Enums;

namespace Icms.Infrastructure.Services;

public class TeamMemberService(
    ITeamMemberRepository teamMemberRepository,
    JoinTeamValidator joinValidator,
    SetRoleValidator setRoleValidator,
    ILogger<TeamMemberService> logger) : ITeamMemberService
{
    public async Task<Result<TeamMemberDto, TeamMemberError>> JoinTeamAsync(
        long churchId, long teamId, JoinTeamRequest request, CancellationToken ct)
    {
        await joinValidator.ValidateAndThrowAsync(request, ct);

        var team = await teamMemberRepository.GetTeamAsync(churchId, teamId, ct);
        if (team is null)
            return Result<TeamMemberDto, TeamMemberError>.Failure(
                TeamMemberError.TeamNotFound(teamId));

        if (team.ParentTeamId is null
            && await teamMemberRepository.HasSubTeamsAsync(churchId, teamId, ct))
            return Result<TeamMemberDto, TeamMemberError>.Failure(
                TeamMemberError.TeamIsCategory(teamId));

        var member = await teamMemberRepository.GetMemberAsync(request.MemberId!.Value, ct);
        if (member is null)
            return Result<TeamMemberDto, TeamMemberError>.Failure(
                TeamMemberError.MemberNotFound(request.MemberId!.Value));

        if (member.ChurchId != churchId)
            return Result<TeamMemberDto, TeamMemberError>.Failure(
                TeamMemberError.MemberChurchMismatch(member.Id, churchId));

        var membership = await teamMemberRepository.GetMembershipAsync(churchId, teamId, member.Id, ct);
        if (membership is not null)
            return Result<TeamMemberDto, TeamMemberError>.Failure(
                TeamMemberError.AlreadyMember(teamId, member.Id));

        var rootTeamId = team.ParentTeamId ?? team.Id;
        var root = rootTeamId == team.Id
            ? team
            : await teamMemberRepository.GetTeamAsync(churchId, rootTeamId, ct);

        if (root is not null && root.MembershipRule == MembershipRule.Single)
        {
            var familyCount = await teamMemberRepository.CountMembershipsInFamilyAsync(
                member.Id, rootTeamId, ct);

            if (familyCount >= 1)
                return Result<TeamMemberDto, TeamMemberError>.Failure(
                    TeamMemberError.SingleMembershipLimit());
        }

        var newMembership = new TeamMember
        {
            TeamId = teamId,
            MemberId = member.Id,
            Role = TeamMemberRole.Member
        };

        try
        {
            var created = await teamMemberRepository.AddAsync(newMembership, ct);

            logger.LogInformation("Member {MemberId} joined team {TeamId}",
                created.MemberId, created.TeamId);

            return Result<TeamMemberDto, TeamMemberError>.Success(
                ToDto(created, MemberNames.Full(member), member.EfgbcId));
        }
        catch (UniqueConstraintViolationException)
        {
            return Result<TeamMemberDto, TeamMemberError>.Failure(
                TeamMemberError.AlreadyMember(teamId, member.Id));
        }
    }

    public async Task<Result<PagedResponse<TeamMemberDto>, TeamMemberError>> GetTeamMembersAsync(
        long churchId, long teamId, PagedRequest paging, CancellationToken ct)
    {
        var team = await teamMemberRepository.GetTeamAsync(churchId, teamId, ct);
        if (team is null)
            return Result<PagedResponse<TeamMemberDto>, TeamMemberError>.Failure(
                TeamMemberError.TeamNotFound(teamId));

        var page = paging.SafePage;

        var memberships = await teamMemberRepository.GetByTeamPagedAsync(
            churchId, teamId, paging.Search, page, paging.PageSize, ct);
        var totalCount = await teamMemberRepository.CountByTeamAsync(
            churchId, teamId, paging.Search, ct);

        return Result<PagedResponse<TeamMemberDto>, TeamMemberError>.Success(
            new PagedResponse<TeamMemberDto>
            {
                Items = memberships.Select(tm => ToDto(tm, MemberNames.Full(tm.Member), tm.Member.EfgbcId)).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = paging.PageSize
            });
    }

    public async Task<Result<TeamMemberDto, TeamMemberError>> SetRoleAsync(
        long churchId, long teamId, long memberId, SetRoleRequest request, CancellationToken ct)
    {
        await setRoleValidator.ValidateAndThrowAsync(request, ct);

        var team = await teamMemberRepository.GetTeamAsync(churchId, teamId, ct);
        if (team is null)
            return Result<TeamMemberDto, TeamMemberError>.Failure(
                TeamMemberError.TeamNotFound(teamId));

        var membership = await teamMemberRepository.GetMembershipAsync(churchId, teamId, memberId, ct);
        if (membership is null)
            return Result<TeamMemberDto, TeamMemberError>.Failure(
                TeamMemberError.NotAMember(teamId, memberId));

        var role = request.Role!.Value;

        if (membership.Role == role)
            return await Idempotent(membership, ct);

        membership.Role = role;

        var updated = await teamMemberRepository.UpdateAsync(membership, ct);

        logger.LogInformation("Member {MemberId} of team {TeamId} is now {Role}",
            updated.MemberId, updated.TeamId, updated.Role);

        return await ToDtoAsync(updated, ct);
    }

    public async Task<Result<TeamMemberDto, TeamMemberError>> RemoveMemberAsync(
        long churchId, long teamId, long memberId, CancellationToken ct)
    {
        var team = await teamMemberRepository.GetTeamAsync(churchId, teamId, ct);
        if (team is null)
            return Result<TeamMemberDto, TeamMemberError>.Failure(
                TeamMemberError.TeamNotFound(teamId));

        var membership = await teamMemberRepository.GetMembershipAsync(churchId, teamId, memberId, ct);
        if (membership is null)
            return Result<TeamMemberDto, TeamMemberError>.Failure(
                TeamMemberError.NotAMember(teamId, memberId));

        membership.IsDeleted = true;

        var removed = await teamMemberRepository.UpdateAsync(membership, ct);

        logger.LogInformation("Member {MemberId} left team {TeamId}",
            removed.MemberId, removed.TeamId);

        return await ToDtoAsync(removed, ct);
    }

    private async Task<Result<TeamMemberDto, TeamMemberError>> Idempotent(
        TeamMember membership, CancellationToken ct)
    {
        logger.LogInformation("Member {MemberId} of team {TeamId} already has role {Role}",
            membership.MemberId, membership.TeamId, membership.Role);

        return await ToDtoAsync(membership, ct);
    }

    private async Task<Result<TeamMemberDto, TeamMemberError>> ToDtoAsync(
        TeamMember membership, CancellationToken ct)
    {
        var member = await teamMemberRepository.GetMemberAsync(membership.MemberId, ct);
        if (member is null)
            return Result<TeamMemberDto, TeamMemberError>.Failure(
                TeamMemberError.MemberNotFound(membership.MemberId));

        return Result<TeamMemberDto, TeamMemberError>.Success(
            ToDto(membership, MemberNames.Full(member), member.EfgbcId));
    }

    private static TeamMemberDto ToDto(TeamMember tm, string memberName, string memberEfgbcId) =>
        new(tm.MemberId, memberName, memberEfgbcId, tm.Role, tm.JoinedAt);
}