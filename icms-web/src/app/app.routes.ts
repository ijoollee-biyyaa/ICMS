import { Routes } from '@angular/router';

import { authGuard } from './guards/auth.guard';
import { roleGuard } from './guards/role.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/landing/landing/landing').then((m) => m.Landing),
  },
  {
    path: 'login',
    title: 'Sign In',
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
  },
  {
    path: 'unauthorized',
    title: 'Not Authorized',
    loadComponent: () =>
      import('./features/auth/unauthorized/unauthorized').then((m) => m.Unauthorized),
  },
  {
    path: 'district',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/district/district-shell/district-shell').then(
        (m) => m.DistrictShell,
      ),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        title: 'District Dashboard',
        loadComponent: () =>
          import('./features/district/dashboard/dashboard').then(
            (m) => m.DistrictDashboard,
          ),
      },
      {
        path: 'profile',
        title: 'District Profile',
        loadComponent: () =>
          import('./features/district/profile/profile').then(
            (m) => m.DistrictProfile,
          ),
      },
      {
        path: 'members',
        title: 'Members',
        loadComponent: () =>
          import('./features/shared/module-placeholder/module-placeholder').then(
            (m) => m.ModulePlaceholder,
          ),
        data: { title: 'Members', icon: 'group', description: 'Every member of the district, their churches, teams and attendance.' },
      },
      {
        path: 'churches',
        title: 'Churches',
        loadComponent: () =>
          import('./features/district/churches/churches').then(
            (m) => m.DistrictChurches,
          ),
      },
      {
        path: 'churches/:id',
        title: 'Church Details',
        loadComponent: () =>
          import('./features/district/churches/church-detail/church-detail').then(
            (m) => m.DistrictChurchDetail,
          ),
      },
      {
        path: 'employees',
        title: 'Office Employees',
        loadComponent: () =>
          import('./features/district/employees/employees').then(
            (m) => m.DistrictEmployees,
          ),
      },
      {
        path: 'departments',
        title: 'Departments',
        loadComponent: () =>
          import('./features/shared/module-placeholder/module-placeholder').then(
            (m) => m.ModulePlaceholder,
          ),
        data: { title: 'Departments', icon: 'account_balance', description: 'Office units that run the district’s work.' },
      },
      {
        path: 'ministers',
        title: 'Ministers Showcase',
        loadComponent: () =>
          import('./features/district/ministers/ministers').then(
            (m) => m.DistrictMinisters,
          ),
      },
      {
        path: 'accounts',
        title: 'Accounts',
        canActivate: [roleGuard(['Admin'])],
        loadComponent: () =>
          import('./features/district/accounts/accounts').then(
            (m) => m.DistrictAccounts,
          ),
      },
      {
        path: 'payments',
        title: 'Payments',
        loadComponent: () =>
          import('./features/shared/module-placeholder/module-placeholder').then(
            (m) => m.ModulePlaceholder,
          ),
        data: { title: 'Payments', icon: 'payments', description: 'Tithes, offerings and salary payments.' },
      },
      {
        path: 'create',
        title: 'Create District',
        canActivate: [roleGuard(['Admin'])],
        loadComponent: () =>
          import('./features/district/create-district/create-district').then(
            (m) => m.CreateDistrictComponent,
          ),
      },
      {
        path: 'settings',
        title: 'Settings',
        canActivate: [roleGuard(['Admin'])],
        loadComponent: () =>
          import('./features/district/settings/settings').then(
            (m) => m.DistrictSettings,
          ),
      },
      {
        path: 'reports',
        title: 'Reports',
        loadComponent: () =>
          import('./features/shared/module-placeholder/module-placeholder').then(
            (m) => m.ModulePlaceholder,
          ),
        data: { title: 'Reports', icon: 'bar_chart', description: 'Complex district reports and analytics.' },
      },
    ],
  },
  {
    path: 'areas',
    title: 'Choose Area',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/auth/area-select/area-select').then((m) => m.AreaSelect),
  },
  {
    path: 'member',
    canActivate: [authGuard, roleGuard(['Member'])],
    loadComponent: () =>
      import('./features/member/member-shell/member-shell').then(
        (m) => m.MemberShell,
      ),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        title: 'Member Dashboard',
        loadComponent: () =>
          import('./features/member/dashboard/dashboard').then(
            (m) => m.MemberDashboard,
          ),
      },
      {
        path: 'profile',
        title: 'My Profile',
        loadComponent: () =>
          import('./features/member/member-profile/member-profile').then(
            (m) => m.MemberProfilePage,
          ),
      },
      {
        path: 'account',
        title: 'Manage Account',
        loadComponent: () =>
          import('./features/member/member-account/member-account').then(
            (m) => m.MemberAccountPage,
          ),
      },
      {
        path: 'attendance',
        title: 'My Attendance',
        loadComponent: () =>
          import('./features/member/my-attendance/my-attendance').then(
            (m) => m.MyAttendance,
          ),
      },
      {
        path: 'payments',
        title: 'My Payments',
        loadComponent: () =>
          import('./features/member/my-payments/my-payments').then(
            (m) => m.MyPayments,
          ),
      },
      {
        path: 'teams',
        title: 'My Teams',
        loadComponent: () =>
          import('./features/member/member-teams/member-teams').then(
            (m) => m.MemberTeams,
          ),
      },
      {
        path: 'teams/:id',
        title: 'Team Meetings',
        loadComponent: () =>
          import(
            './features/member/member-team-meetings/member-team-meetings'
          ).then((m) => m.MemberTeamMeetings),
      },
    ],
  },
  {
    path: 'church',
    canActivate: [authGuard, roleGuard(['ChurchAdmin'])],
    loadComponent: () =>
      import('./features/church/church-shell/church-shell').then(
        (m) => m.ChurchShell,
      ),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        title: 'Dashboard',
        loadComponent: () =>
          import('./features/church/church-dashboard/church-dashboard').then(
            (m) => m.ChurchDashboard,
          ),
      },
      {
        path: 'profile',
        title: 'Church Profile',
        loadComponent: () =>
          import('./features/church/church-profile/church-profile').then(
            (m) => m.ChurchProfile,
          ),
      },
      {
        path: 'members',
        title: 'Members',
        loadComponent: () =>
          import('./features/church/church-members/church-members').then(
            (m) => m.ChurchMembers,
          ),
      },
      {
        path: 'members/register',
        title: 'Register Member',
        loadComponent: () =>
          import(
            './features/church/church-members/member-register/member-register'
          ).then((m) => m.MemberRegister),
      },
      {
        path: 'members/:id',
        title: 'Member',
        loadComponent: () =>
          import(
            './features/church/church-members/member-detail/member-detail'
          ).then((m) => m.MemberDetailPage),
      },
      {
        path: 'clearance',
        title: 'Register by Clearance',
        loadComponent: () =>
          import('./features/church/clearance/clearance').then(
            (m) => m.ClearancePage,
          ),
      },
      {
        path: 'teams',
        title: 'Teams',
        loadComponent: () =>
          import('./features/church/church-teams/church-teams').then(
            (m) => m.ChurchTeams,
          ),
      },
      {
        path: 'teams/create',
        title: 'Create Team',
        loadComponent: () =>
          import('./features/church/teams/team-create/team-create.component').then(
            (m) => m.TeamCreateComponent,
          ),
      },
      {
        path: 'teams/attendance',
        title: 'Team Attendance',
        loadComponent: () =>
          import('./features/church/teams/team-attendance/team-attendance.component').then(
            (m) => m.TeamAttendanceComponent,
          ),
      },
      {
        path: 'teams/:id',
        title: 'Team Detail',
        loadComponent: () =>
          import(
            './features/church/church-teams/team-detail/team-detail'
          ).then((m) => m.TeamDetailPage),
      },
      {
        path: 'employees',
        title: 'Employees',
        loadComponent: () =>
          import('./features/church/church-employees/church-employees').then(
            (m) => m.ChurchEmployees,
          ),
      },
      {
        path: 'departments',
        title: 'Departments',
        loadComponent: () =>
          import(
            './features/church/church-departments/church-departments'
          ).then((m) => m.ChurchDepartments),
      },
      {
        path: 'accounts',
        title: 'Accounts',
        loadComponent: () =>
          import('./features/church/church-accounts/church-accounts').then(
            (m) => m.ChurchAccounts,
          ),
      },
      {
        path: 'settings',
        title: 'Settings',
        loadComponent: () =>
          import('./features/church/church-settings/church-settings').then(
            (m) => m.ChurchSettings,
          ),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];