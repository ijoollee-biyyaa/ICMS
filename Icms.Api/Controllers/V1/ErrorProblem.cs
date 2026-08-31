using Microsoft.AspNetCore.Mvc;
using Icms.Application.Common;

namespace Icms.Api.Controllers.V1;

public static class ErrorProblem
{
    public static ProblemDetails ToProblem(this IAppError error, HttpRequest request) =>
        new()
        {
            Status = error.Status,
            Title = error.Code,
            Detail = error.Message,
            Instance = request.Path
        };

    public static IActionResult ToResult(this IAppError error, HttpRequest request) =>
        new ObjectResult(error.ToProblem(request))
        {
            StatusCode = error.Status
        };
}