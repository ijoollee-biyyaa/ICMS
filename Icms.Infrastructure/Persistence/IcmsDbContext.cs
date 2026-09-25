using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Icms.Domain.Entities;
using Icms.Infrastructure.Identity;

namespace Icms.Infrastructure.Persistence;

public class IcmsDbContext(DbContextOptions<IcmsDbContext> options) : IdentityDbContext<User>(options)
{
    public DbSet<District> Districts => Set<District>();
    public DbSet<Church> Churches => Set<Church>();
    public DbSet<ServiceTime> ServiceTimes => Set<ServiceTime>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<MemberFamily> MemberFamilies => Set<MemberFamily>();
    public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
    public DbSet<MemberDocument> MemberDocuments => Set<MemberDocument>();
    public DbSet<BaptismRecord> BaptismRecords => Set<BaptismRecord>();
    public DbSet<LearningRecord> LearningRecords => Set<LearningRecord>();
    public DbSet<DeathRecord> DeathRecords => Set<DeathRecord>();
    public DbSet<PrayerRequest> PrayerRequests => Set<PrayerRequest>();
    public DbSet<Transfer> Transfers => Set<Transfer>();
    public DbSet<TransferSnapshot> TransferSnapshots => Set<TransferSnapshot>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<TeamAttendance> TeamAttendances => Set<TeamAttendance>();
    public DbSet<TeamMeeting> TeamMeetings => Set<TeamMeeting>();
    public DbSet<TeamPayment> TeamPayments => Set<TeamPayment>();
    public DbSet<TeamActivity> TeamActivities => Set<TeamActivity>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Tithe> Tithes => Set<Tithe>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<DepartmentEmployee> DepartmentEmployees => Set<DepartmentEmployee>();
    public DbSet<DepartmentActivity> DepartmentActivities => Set<DepartmentActivity>();
    public DbSet<UpgradeApplication> UpgradeApplications => Set<UpgradeApplication>();
    public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IcmsDbContext).Assembly);
    }
}