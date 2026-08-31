using FluentValidation;
using Icms.Application.DTOs;
using Icms.Domain.Enums;

namespace Icms.Application.Districts;

public class CreateDepartmentValidator : AbstractValidator<CreateDepartmentRequest>
{
    public CreateDepartmentValidator()
    {
        RuleFor(x => x.Name)
            .NotNull().WithMessage("A department name is required.")
            .NotEmpty().WithMessage("A department name is required.")
            .MaximumLength(150).WithMessage("Department name can be at most 150 characters.");

        RuleFor(x => x.Type)
            .NotNull().WithMessage("A department type is required.")
            .IsInEnum().WithMessage("Invalid department type.");
    }
}

public class UpdateDepartmentValidator : AbstractValidator<UpdateDepartmentRequest>
{
    public UpdateDepartmentValidator()
    {
        RuleFor(x => x.Name)
            .NotNull().WithMessage("A department name is required.")
            .NotEmpty().WithMessage("A department name is required.")
            .MaximumLength(150).WithMessage("Department name can be at most 150 characters.");

        RuleFor(x => x.Type)
            .NotNull().WithMessage("A department type is required.")
            .IsInEnum().WithMessage("Invalid department type.");
    }
}

public class AddDepartmentEmployeeValidator : AbstractValidator<AddDepartmentEmployeeRequest>
{
    public AddDepartmentEmployeeValidator()
    {
        RuleFor(x => x.EmployeeId)
            .NotNull().WithMessage("An employee id is required.");

        RuleFor(x => x.Role)
            .NotNull().WithMessage("A role is required.")
            .IsInEnum().WithMessage("Invalid role.");
    }
}

public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeValidator()
    {
        RuleFor(x => x.Position)
            .NotNull().WithMessage("A position is required.")
            .NotEmpty().WithMessage("A position is required.")
            .MaximumLength(100).WithMessage("Position can be at most 100 characters.");

        RuleFor(x => x.EmploymentType)
            .NotNull().WithMessage("An employment type is required.")
            .IsInEnum().WithMessage("Invalid employment type.");

        RuleFor(x => x.MinisterTitle)
            .IsInEnum().WithMessage("Invalid minister title.");

        RuleFor(x => x.Salary)
            .GreaterThan(0).WithMessage("Salary must be greater than zero.")
            .Must(v => v is null || decimal.Round(v.Value, 2) == v.Value)
            .WithMessage("Salary can have at most 2 decimal places.");

        RuleFor(x => x.FirstName)
            .NotEmpty().When(x => x.MemberId is null)
            .WithMessage("Full name is required when hiring from outside.");

        RuleFor(x => x.FatherName)
            .NotEmpty().When(x => x.MemberId is null)
            .WithMessage("Full name is required when hiring from outside.");

        RuleFor(x => x.GrandfatherName)
            .NotEmpty().When(x => x.MemberId is null)
            .WithMessage("Full name is required when hiring from outside.");
    }
}

public class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeRequest>
{
    public UpdateEmployeeValidator()
    {
        RuleFor(x => x.Position)
            .NotNull().WithMessage("A position is required.")
            .NotEmpty().WithMessage("A position is required.")
            .MaximumLength(100).WithMessage("Position can be at most 100 characters.");

        RuleFor(x => x.MinisterTitle)
            .IsInEnum().WithMessage("Invalid minister title.");

        RuleFor(x => x.Salary)
            .GreaterThan(0).WithMessage("Salary must be greater than zero.")
            .Must(v => v is null || decimal.Round(v.Value, 2) == v.Value)
            .WithMessage("Salary can have at most 2 decimal places.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid employee status.");
    }
}