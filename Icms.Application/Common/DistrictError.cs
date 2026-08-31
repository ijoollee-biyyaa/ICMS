namespace Icms.Application.Common;

public sealed record DistrictError(string Code, string Message, int Status) : IAppError
{
    public static DistrictError NotFound(long id) =>
        new("district_not_found", $"District with id {id} was not found.", 404);

    public static DistrictError CodeAlreadyExists(string code) =>
        new("district_code_exists", $"A district with code {code} already exists.", 409);

    public static DistrictError CannotDeleteWithChurches(long id) =>
        new("district_has_churches",
            $"District {id} still has churches; delete or move them before deleting the district.", 409);
}