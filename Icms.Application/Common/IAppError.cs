namespace Icms.Application.Common;

public interface IAppError
{
    string Code { get; }
    string Message { get; }
    int Status { get; }
}