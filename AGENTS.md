# AGENTS.md — EFGBC ICMS

## Domain rules (do not break)

- **Team ≠ Department**: a *Team* is a ministry group that members serve in if assigned (Choir, Worship, Women, Youth, Bible study — `Team`/`TeamMember`). A *Department* is a structured office unit that has hired employees (Finance, HR, Administration, Media — `Department`/`DepartmentEmployee`). Never model a ministry as a department or vice versa.
- **Dual employment (church + district) is normal**: most often a person is hired at a church first, then also at the district office. Both employment records exist — the church record stays (legal data), but the **district pays**: when a district hire's member is already a church employee, set `SalaryPaidBy = District` on all that member's church employee records ("paid from district" badge). Symmetric: when hiring a church employee whose member is already a district employee, the new record starts with `SalaryPaidBy = District`. This applies mainly to fulltime ministers; other types rarely work both places. If the member later leaves the district (no remaining active district employment), revert their church records to `SalaryPaidBy = Church` — the church pays again.
- **Counting rule**: district-level employee counts must not double-count a person employed both at the district and at a church — dedupe by `MemberId` (person). Within-office and within-church counts are unaffected. Apply when building the district overview/finance reports.

## Conventions (rules, do not break)

- **Full names everywhere**: never show only `FirstName` for a person. Use the full name (First + Father + Grandfather).
  - In-memory DTO builds: `MemberNames.Full(member)` from `Icms.Application.Common` (null-safe for employees without a member: check `member is null` first).
  - EF SQL projections: inline `m.FirstName + " " + m.FatherName + " " + m.GrandfatherName` (EF cannot translate the static helper).
  - Applies to member/team/attendance/payment/dashboard/employee/department responses.

## EF Core rules

- Do all `Where`/`OrderBy` on entity properties **before** `Select` — once projected into a record/anonymous type, the query cannot be translated further (caused repeated 500s).
- `SaveChangesAsync` does not fill navigation properties. After `Add`/`Update`, `LoadAsync` navigations needed for the response DTO (e.g. `Entry(x).Reference(e => e.Member).LoadAsync(ct)`), else null-ref 500 after a successful insert.
- Nullable FK comparisons: `== null` compiles to `= NULL` in SQL — widen conditions.
- Always kill the running app before `dotnet build` (DLL locks). And **rebuild before `dotnet ef database update --no-build`** — stale compiled snapshots in the DLL cause false `PendingModelChangesWarning`.
- Never inline JSON in PowerShell `-d "..."` — write payload files and use `-d "@file"`.
- Enums are stored as ints; adding/renaming enum members needs no migration as long as int values don't change. New enum values (e.g. `DepartmentType.Administrative`) need no migration.
- Validators auto-register via `AddValidatorsFromAssembly`.
- Operations: app http://localhost:5171 (Scalar /scalar/v1); kill `Get-Process | ? { $_.ProcessName -like "*Icms*" } | Stop-Process`; start `dotnet run --project Icms.Api --no-build` from C:\Users\ijool\ICMS; logs in C:\Users\ijool\AppData\Local\Temp\opencode\icms-run.{log,err}; psql: "C:\Program Files\PostgreSQL\18\bin\psql.exe" -h 
localhost -U postgres -d IcmsDb, PGPASSWORD='<see appsettings.Development.json>', SQL via `-f` files.