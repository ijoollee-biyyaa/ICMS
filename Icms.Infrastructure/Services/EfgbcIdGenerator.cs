using Microsoft.EntityFrameworkCore;
using Icms.Application.Interfaces;
using Icms.Infrastructure.Persistence;

namespace Icms.Infrastructure.Services;

public class EfgbcIdGenerator(IcmsDbContext dbContext) : IEfgbcIdGenerator
{
    public async Task<string> NextAsync(string districtCode, CancellationToken ct)
    {
        var next = await dbContext.Database
            .SqlQueryRaw<long>("SELECT nextval('\"EfgbcMemberIdSeq\"') AS \"Value\"")
            .SingleAsync(ct);

        return $"EFGBC-{districtCode}-{next:D6}";
    }
}