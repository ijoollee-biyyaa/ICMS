namespace Icms.Application.Common;

public sealed record DistrictDepartmentError(string Code, string Message, int Status) : IAppError
{
    public static DistrictDepartmentError DistrictNotFound(long districtId) =>
        new("district_not_found", $"District with id {districtId} was not found.", 404);

    public static DistrictDepartmentError NotFound(long departmentId) =>
        new("department_not_found", $"Department with id {departmentId} was not found.", 404);

    public static DistrictDepartmentError HeadNotOfficeEmployee(long employeeId) =>
        new("department_head_not_office_employee",
            $"Employee {employeeId} is not a district office employee.", 409);

    public static DistrictDepartmentError EmployeeNotOfficeEmployee(long employeeId) =>
        new("department_employee_not_office_employee",
            $"Employee {employeeId} is not a district office employee.", 409);

    public static DistrictDepartmentError EmployeeAlreadyInDepartment(long employeeId, long departmentId) =>
        new("department_employee_exists",
            $"Employee {employeeId} is already assigned to department {departmentId}.", 409);

    public static DistrictDepartmentError DepartmentEmployeeNotFound(long id) =>
        new("department_employee_not_found",
            $"Department assignment with id {id} was not found.", 404);
}

public sealed record DistrictEmployeeError(string Code, string Message, int Status) : IAppError
{
    public static DistrictEmployeeError DistrictNotFound(long districtId) =>
        new("district_not_found", $"District with id {districtId} was not found.", 404);

    public static DistrictEmployeeError NotFound(long employeeId) =>
        new("employee_not_found", $"Employee with id {employeeId} was not found.", 404);

    public static DistrictEmployeeError MinisterRequiresMember() =>
        new("employee_minister_requires_member",
            "A full-time minister must be linked to a church member (ministers come from the church).", 409);

    public static DistrictEmployeeError MemberNotFound(long memberId) =>
        new("employee_member_not_found",
            $"Member with id {memberId} was not found in this district.", 404);

    public static DistrictEmployeeError MemberNotLocalChurch(long memberId) =>
        new("employee_member_not_local_church",
            $"Member {memberId} is not in a local church; only members of local churches can be executives.", 409);

    public static DistrictEmployeeError PresidentMustBeMinister() =>
        new("employee_president_must_be_minister",
            "The district president must be a full-time minister of a local church.", 409);

    public static DistrictEmployeeError VicePresidentMustBeMinister() =>
        new("employee_vice_president_must_be_minister",
            "The vice president must be a full-time minister of a local church.", 409);

    public static DistrictEmployeeError PresidentExists(long employeeId) =>
        new("employee_president_exists",
            $"A district president already exists (employee {employeeId}); there can only be one.", 409);

    public static DistrictEmployeeError VicePresidentExists(long employeeId) =>
        new("employee_vice_president_exists",
            $"A vice president already exists (employee {employeeId}); there can only be one.", 409);

    public static DistrictEmployeeError AccountCreationFailed(string message) =>
        new("employee_account_create_failed",
            $"Account could not be created: {message}", 409);
}