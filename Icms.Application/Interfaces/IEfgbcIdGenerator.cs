namespace Icms.Application.Interfaces;

public interface IEfgbcIdGenerator
{
    Task<string> NextAsync(string districtCode, CancellationToken ct);
}