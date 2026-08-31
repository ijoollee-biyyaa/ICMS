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
        path: 'employees',
        title: 'Office Employees',
        loadComponent: () =>
          import('./features/shared/module-placeholder/module-placeholder').then(
            (m) => m.ModulePlaceholder,
          ),
        data: { title: 'Office Employees', icon: 'badge', description: 'The district office team, executive positions and paid-from-district badges.' },
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
        title: 'Ministers',
        loadComponent: () =>
          import('./features/shared/module-placeholder/module-placeholder').then(
            (m) => m.ModulePlaceholder,
          ),
        data: { title: 'Ministers', icon: 'local_church', description: 'Full-time ministers across the district, one row per person.' },
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
          import('./features/shared/module-placeholder/module-placeholder').then(
            (m) => m.ModulePlaceholder,
          ),
        data: { title: 'Member Dashboard', icon: 'person', description: 'Your membership home — teams, attendance and giving.', backLink: '' },
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
        path: 'members/:id',
        title: 'Member',
        loadComponent: () =>
          import(
            './features/church/church-members/member-detail/member-detail'
          ).then((m) => m.MemberDetailPage),
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