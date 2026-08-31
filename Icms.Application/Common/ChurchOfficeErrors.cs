namespace Icms.Application.Common;

public sealed record ChurchEmployeeError(string Code, string Message, int Status) : IAppError
{
    public static ChurchEmployeeError ChurchNotFound(long churchId) =>
        new("church_not_found", $"Church {churchId} was not found.", 404);

    public static ChurchEmployeeError NotFound(long employeeId) =>
        new("employee_not_found", $"Church employee {employeeId} was not found.", 404);

    public static ChurchEmployeeError MinisterRequiresMember() =>
        new("employee_minister_requires_member",
            "A full-time minister must be linked to a church member (ministers come from the church).", 409);

    public static ChurchEmployeeError MemberNotFound(long memberId) =>
        new("employee_member_not_found",
            $"Member {memberId} does not belong to this church.", 409);

    public static ChurchEmployeeError MemberAlreadyHired(long memberId, long churchId) =>
        new("church_employee_member_exists",
            $"Member {memberId} is already an employee of church {churchId}.", 409);

    public static ChurchEmployeeError AccountCreationFailed(string message) =>
        new("church_employee_account_creation_failed",
            $"Could not create the employee account: {message}", 400);
}
public sealed record ChurchDepartmentError(string Code, string Message, int Status) : IAppError
{
    public static ChurchDepartmentError ChurchNotFound(long churchId) =>
        new("church_not_found", $"Church {churchId} was not found.", 404);

    public static ChurchDepartmentError NotFound(long departmentId) =>
        new("department_not_found", $"Church department {departmentId} was not found.", 404);

    public static ChurchDepartmentError HeadNotChurchEmployee(long employeeId, long churchId) =>
        new("department_head_not_church_employee",
            $"Employee {employeeId} is not an employee of church {churchId}.", 409);

    public static ChurchDepartmentError EmployeeNotOfChurch(long employeeId, long churchId) =>
        new("church_department_employee_not_of_church",
            $"Employee {employeeId} is not an employee of church {churchId}.", 409);

    public static ChurchDepartmentError EmployeeAlreadyInDepartment(long employeeId, long departmentId) =>
        new("church_department_employee_exists",
            $"Employee {employeeId} is already assigned to department {departmentId}.", 409);

    public static ChurchDepartmentError DepartmentEmployeeNotFound(long id) =>
        new("church_department_employee_not_found",
            $"Department assignment with id {id} was not found.", 404);
}