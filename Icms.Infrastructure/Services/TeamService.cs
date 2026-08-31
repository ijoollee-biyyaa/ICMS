using FluentValidation;
using Microsoft.Extensions.Logging;
using Icms.Application.Common;
using Icms.Application.DTOs;
using Icms.Application.Exceptions;
using Icms.Application.Interfaces;
using Icms.Application.Teams;
using Icms.Domain.Entities;

namespace Icms.Infrastructure.Services;

public class TeamService(
    ITeamRepository teamRepository,
    CreateTeamValidator createValidator,
    UpdateTeamValidator updateValidator,
    ILogger<TeamService> logger) : ITeamService
{
    public async Task<Result<TeamResponseDto, TeamError>> CreateTeamAsync(
        long churchId, CreateTeamRequest request, CancellationToken ct)
    {
        await createValidator.ValidateAndThrowAsync(request, ct);

        var church = await teamRepository.GetChurchAsync(churchId, ct);
        if (church is null)
            return Result<TeamResponseDto, TeamError>.Failure(
                TeamError.ChurchNotFound(churchId));

        if (request.ParentTeamId is not null)
        {
            var parent = await teamRepository.GetByIdAsync(churchId, request.ParentTeamId.Value, ct);
            if (parent is null)
                return Result<TeamResponseDto, TeamError>.Failure(
                    TeamError.ParentNotFound(request.ParentTeamId.Value));

            if (parent.ParentTeamId is not null)
                return Result<TeamResponseDto, TeamError>.Failure(
                    TeamError.ParentIsSubTeam());

            if (await teamRepository.HasMembersAsync(churchId, parent.Id, ct))
                return Result<TeamResponseDto, TeamError>.Failure(
                    TeamError.ParentHasMembers());

            request = request with { MembershipRule = parent.MembershipRule };
        }

        if (await teamRepository.NameExistsAsync(churchId, request.Name, ct))
            return Result<TeamResponseDto, TeamError>.Failure(
                TeamError.NameAlreadyExists(request.Name));

        var team = new Team
        {
            ChurchId = churchId,
            Name = request.Name,
            ParentTeamId = request.ParentTeamId,
            MembershipRule = request.MembershipRule!.Value
        };

        try
        {
            var created = await teamRepository.AddAsync(team, ct);

            logger.LogInformation("Created team {TeamId} ({Name}) at church {ChurchId}",
                created.Id, created.Name, created.ChurchId);

            return Result<TeamResponseDto, TeamError>.Success(ToDto(created));
        }
        catch (UniqueConstraintViolationException)
        {
            return Result<TeamResponseDto, TeamError>.Failure(
                TeamError.NameAlreadyExists(request.Name));
        }
    }

    public async Task<Result<TeamResponseDto, TeamError>> GetTeamByIdAsync(
        long churchId, long id, CancellationToken ct)
    {
        var team = await teamRepository.GetByIdAsync(churchId, id, ct);
        if (team is null)
            return Result<TeamResponseDto, TeamError>.Failure(TeamError.NotFound(id));

        return Result<TeamResponseDto, TeamError>.Success(ToDto(team));
    }

    public async Task<Result<TeamDetailDto, TeamError>> GetTeamDetailAsync(
        long churchId, long id, CancellationToken ct)
    {
        var team = await teamRepository.GetByIdAsync(churchId, id, ct);
        if (team is null)
            return Result<TeamDetailDto, TeamError>.Failure(TeamError.NotFound(id));

        var subTeamCount = await teamRepository.CountSubTeamsAsync(churchId, id, ct);
        var memberCount = await teamRepository.CountMembersAsync(churchId, id, ct);

        var dto = new TeamDetailDto(
            team.Id, team.ChurchId, team.ParentTeamId, team.MembershipRule, team.Name,
            team.CreatedAt, subTeamCount, memberCount);

        return Result<TeamDetailDto, TeamError>.Success(dto);
    }

    public async Task<Result<TeamResponseDto, TeamError>> UpdateTeamAsync(
        long churchId, long id, UpdateTeamRequest request, CancellationToken ct)
    {
        await updateValidator.ValidateAndThrowAsync(request, ct);

        var team = await teamRepository.GetByIdAsync(churchId, id, ct);
        if (team is null)
            return Result<TeamResponseDto, TeamError>.Failure(TeamError.NotFound(id));

        if (await teamRepository.NameExistsAsync(churchId, request.Name, ct)
            && !string.Equals(request.Name, team.Name, StringComparison.OrdinalIgnoreCase))
            return Result<TeamResponseDto, TeamError>.Failure(
                TeamError.NameAlreadyExists(request.Name));

        team.Name = request.Name;

        try
        {
            var updated = await teamRepository.UpdateAsync(team, ct);

            logger.LogInformation("Updated team {TeamId} ({Name})", updated.Id, updated.Name);

            return Result<TeamResponseDto, TeamError>.Success(ToDto(updated));
        }
        catch (UniqueConstraintViolationException)
        {
            return Result<TeamResponseDto, TeamError>.Failure(
                TeamError.NameAlreadyExists(request.Name));
        }
    }

    public async Task<Result<TeamResponseDto, TeamError>> DeleteTeamAsync(
        long churchId, long id, CancellationToken ct)
    {
        var team = await teamRepository.GetByIdAsync(churchId, id, ct);
        if (team is null)
            return Result<TeamResponseDto, TeamError>.Failure(TeamError.NotFound(id));

        if (await teamRepository.HasSubTeamsAsync(churchId, id, ct))
            return Result<TeamResponseDto, TeamError>.Failure(
                TeamError.CannotDeleteWithSubTeams());

        if (await teamRepository.HasMembersAsync(churchId, id, ct))
            return Result<TeamResponseDto, TeamError>.Failure(
                TeamError.CannotDeleteWithMembers());

        team.IsDeleted = true;

        var deleted = await teamRepository.UpdateAsync(team, ct);

        logger.LogInformation("Deleted team {TeamId} ({Name})", deleted.Id, deleted.Name);

        return Result<TeamResponseDto, TeamError>.Success(ToDto(deleted));
    }

    public async Task<Result<PagedResponse<TeamResponseDto>, TeamError>> GetTeamsAsync(
        long churchId, PagedRequest paging, CancellationToken ct)
    {
        var church = await teamRepository.GetChurchAsync(churchId, ct);
        if (church is null)
            return Result<PagedResponse<TeamResponseDto>, TeamError>.Failure(
                TeamError.ChurchNotFound(churchId));

        var page = paging.SafePage;
        var pageSize = paging.PageSize;

        var teams = await teamRepository.GetByChurchPagedAsync(churchId, paging.Search, page, pageSize, ct);
        var totalCount = await teamRepository.CountByChurchAsync(churchId, paging.Search, ct);

        return Result<PagedResponse<TeamResponseDto>, TeamError>.Success(
            new PagedResponse<TeamResponseDto>
            {
                Items = teams.Select(ToDto).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
    }

    private static TeamResponseDto ToDto(Team t) =>
        new(t.Id, t.ChurchId, t.ParentTeamId, t.MembershipRule, t.Name, t.CreatedAt);
}