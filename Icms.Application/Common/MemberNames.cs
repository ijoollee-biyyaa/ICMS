using Icms.Domain.Entities;

namespace Icms.Application.Common;

public static class MemberNames
{
    public static string Full(Member m) =>
        $"{m.FirstName} {m.FatherName} {m.GrandfatherName}".Trim();
}