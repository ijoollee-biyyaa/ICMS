import { Service, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { map } from 'rxjs/operators';

import { environment } from '../../environments/environment';
import { PagedResponse } from '../models/district';
import {
  Church,
  ChurchDetail,
  CreateChurchRequest,
  CreateChurchResponse,
  UpdateChurchRequest,
} from '../models/church';
import {
  Member,
  CreateMemberRequest,
  UpdateMemberRequest,
  MemberDashboard,
  MemberStats,
  MemberAccountInfo,
  IssueMemberCredentials,
} from '../models/member';
import { Department, DepartmentEmployee } from '../models/department';
import {
  UpsertDepartmentRequest,
  AddDepartmentEmployeeRequest,
} from '../models/department';
import { UserAccount, RoleChangeRequest, LockAccountRequest } from '../models/account';
import { EmployeeRow, CreateEmployeeRequest, UpdateEmployeeRequest } from '../models/district';
import {
  Team,
  TeamDetail,
  TeamMember,
  CreateTeamRequest,
  UpdateTeamRequest,
  JoinTeamRequest,
  SetRoleRequest,
  TeamAttendanceRecord,
  TeamAttendanceReport,
  SaveAttendanceRequest,
  SaveAttendanceResult,
  TeamPayment,
  TeamPaymentSummary,
  RecordPaymentRequest,
  UpdatePaymentRequest,
} from '../models/team';

/**
 * Central client for the church domain: churches under a district, plus the
 * church office (employees, departments, accounts) and church members/teams.
 */
@Service()
export class ChurchService {
  private readonly http = inject(HttpClient);
  private readonly districtBase = `${environment.apiUrl}/districts`;
  private readonly memberBase = `${environment.apiUrl}/members`;

  private paging(page: number, pageSize: number): HttpParams {
    return new HttpParams()
      .set('page', String(page))
      .set('pageSize', String(pageSize));
  }

  // ---- Churches --------------------------------------------------------

  getChurches(districtId: number, page = 1, pageSize = 50) {
    return this.http
      .get<PagedResponse<Church>>(`${this.districtBase}/${districtId}/churches`, {
        params: this.paging(page, pageSize),
      })
      .pipe(map((p) => p.items));
  }

  getChurch(districtId: number, churchId: number) {
    return this.http.get<ChurchDetail>(
      `${this.districtBase}/${districtId}/churches/${churchId}`,
    );
  }

  createChurch(districtId: number, body: CreateChurchRequest) {
    return this.http.post<CreateChurchResponse>(
      `${this.districtBase}/${districtId}/churches`,
      body,
    );
  }

  updateChurch(districtId: number, churchId: number, body: UpdateChurchRequest) {
    return this.http.put<Church>(
      `${this.districtBase}/${districtId}/churches/${churchId}`,
      body,
    );
  }

  deleteChurch(districtId: number, churchId: number) {
    return this.http.delete<void>(
      `${this.districtBase}/${districtId}/churches/${churchId}`,
    );
  }

  // ---- Members (global) ------------------------------------------------

  getMembers(churchId: number, page = 1, pageSize = 50, search: string | null = null) {
    let params = this.paging(page, pageSize);
    if (churchId) {
      params = params.set('churchId', String(churchId));
    }
    if (search) {
      params = params.set('search', search);
    }
    return this.http.get<PagedResponse<Member>>(this.memberBase, { params });
  }

  getMemberStats(churchId: number) {
    let params = new HttpParams();
    if (churchId) {
      params = params.set('churchId', String(churchId));
    }
    return this.http.get<MemberStats>(`${this.memberBase}/stats`, { params });
  }

  getMemberAccount(memberId: number) {
    return this.http.get<MemberAccountInfo>(`${this.memberBase}/${memberId}/account`);
  }

  issueMemberCredentials(memberId: number) {
    return this.http.post<IssueMemberCredentials>(
      `${this.memberBase}/${memberId}/account`,
      null,
    );
  }

  getMember(memberId: number) {
    return this.http.get<Member>(`${this.memberBase}/${memberId}`);
  }

  createMember(body: CreateMemberRequest) {
    return this.http.post<Member>(this.memberBase, body);
  }

  updateMember(memberId: number, body: UpdateMemberRequest) {
    return this.http.put<Member>(`${this.memberBase}/${memberId}`, body);
  }

  deleteMember(memberId: number) {
    return this.http.delete<void>(`${this.memberBase}/${memberId}`);
  }

  getMemberDashboard(memberId: number) {
    return this.http.get<MemberDashboard>(
      `${this.memberBase}/${memberId}/dashboard`,
    );
  }

  // ---- Church teams ------------------------------------------------------

  getTeams(churchId: number, page = 1, pageSize = 50) {
    return this.http.get<PagedResponse<Team>>(this.churchTeams(churchId), {
      params: this.paging(page, pageSize),
    });
  }

  getTeam(churchId: number, teamId: number) {
    return this.http.get<TeamDetail>(
      `${this.churchTeams(churchId)}/${teamId}`,
    );
  }

  createTeam(churchId: number, body: CreateTeamRequest) {
    return this.http.post<Team>(this.churchTeams(churchId), body);
  }

  updateTeam(churchId: number, teamId: number, body: UpdateTeamRequest) {
    return this.http.put<Team>(
      `${this.churchTeams(churchId)}/${teamId}`,
      body,
    );
  }

  deleteTeam(churchId: number, teamId: number) {
    return this.http.delete<void>(`${this.churchTeams(churchId)}/${teamId}`);
  }

  // ---- Team members ------------------------------------------------------

  getTeamMembers(churchId: number, teamId: number, page = 1, pageSize = 50) {
    return this.http.get<PagedResponse<TeamMember>>(
      `${this.churchTeams(churchId)}/${teamId}/members`,
      { params: this.paging(page, pageSize) },
    );
  }

  joinTeam(churchId: number, teamId: number, memberId: number) {
    const body: JoinTeamRequest = { memberId };
    return this.http.post<TeamMember>(
      `${this.churchTeams(churchId)}/${teamId}/members`,
      body,
    );
  }

  setTeamMemberRole(
    churchId: number,
    teamId: number,
    memberId: number,
    role: 'Member' | 'Leader',
  ) {
    const body: SetRoleRequest = { role };
    return this.http.put<TeamMember>(
      `${this.churchTeams(churchId)}/${teamId}/members/${memberId}/role`,
      body,
    );
  }

  removeTeamMember(churchId: number, teamId: number, memberId: number) {
    return this.http.delete<void>(
      `${this.churchTeams(churchId)}/${teamId}/members/${memberId}`,
    );
  }

  // ---- Team attendance ---------------------------------------------------

  saveAttendance(
    churchId: number,
    teamId: number,
    body: SaveAttendanceRequest,
  ) {
    return this.http.post<SaveAttendanceResult>(
      `${this.churchTeams(churchId)}/${teamId}/attendance`,
      body,
    );
  }

  getAttendance(
    churchId: number,
    teamId: number,
    from: string | null,
    to: string | null,
    page = 1,
    pageSize = 50,
  ) {
    let params = this.paging(page, pageSize);
    if (from) params = params.set('from', from);
    if (to) params = params.set('to', to);
    return this.http.get<PagedResponse<TeamAttendanceRecord>>(
      `${this.churchTeams(churchId)}/${teamId}/attendance`,
      { params },
    );
  }

  getAttendanceSummary(
    churchId: number,
    teamId: number,
    from: string | null,
    to: string | null,
  ) {
    let params = new HttpParams();
    if (from) params = params.set('from', from);
    if (to) params = params.set('to', to);
    return this.http.get<TeamAttendanceReport>(
      `${this.churchTeams(churchId)}/${teamId}/attendance/summary`,
      { params },
    );
  }

  // ---- Team payments -----------------------------------------------------

  recordPayment(churchId: number, teamId: number, body: RecordPaymentRequest) {
    return this.http.post<TeamPayment>(
      `${this.churchTeams(churchId)}/${teamId}/payments`,
      body,
    );
  }

  getPayments(
    churchId: number,
    teamId: number,
    fromMonth: string | null,
    toMonth: string | null,
    page = 1,
    pageSize = 50,
  ) {
    let params = this.paging(page, pageSize);
    if (fromMonth) params = params.set('fromMonth', fromMonth);
    if (toMonth) params = params.set('toMonth', toMonth);
    return this.http.get<PagedResponse<TeamPayment>>(
      `${this.churchTeams(churchId)}/${teamId}/payments`,
      { params },
    );
  }

  updatePayment(
    churchId: number,
    teamId: number,
    paymentId: number,
    body: UpdatePaymentRequest,
  ) {
    return this.http.put<TeamPayment>(
      `${this.churchTeams(churchId)}/${teamId}/payments/${paymentId}`,
      body,
    );
  }

  getPaymentSummary(churchId: number, teamId: number) {
    return this.http.get<TeamPaymentSummary[]>(
      `${this.churchTeams(churchId)}/${teamId}/payments/summary`,
    );
  }

  // ---- Church office employees -----------------------------------------

  getEmployees(churchId: number, page = 1, pageSize = 50) {
    return this.http.get<PagedResponse<EmployeeRow>>(
      `${this.churchEmployees(churchId)}`,
      { params: this.paging(page, pageSize) },
    );
  }

  createEmployee(churchId: number, body: CreateEmployeeRequest) {
    return this.http.post<EmployeeRow>(
      `${this.churchEmployees(churchId)}`,
      body,
    );
  }

  updateEmployee(
    churchId: number,
    employeeId: number,
    body: UpdateEmployeeRequest,
  ) {
    return this.http.put<EmployeeRow>(
      `${this.churchEmployees(churchId)}/${employeeId}`,
      body,
    );
  }

  deleteEmployee(churchId: number, employeeId: number) {
    return this.http.delete<EmployeeRow>(
      `${this.churchEmployees(churchId)}/${employeeId}`,
    );
  }

  // ---- Church departments ----------------------------------------------

  getDepartments(churchId: number, page = 1, pageSize = 50) {
    return this.http.get<PagedResponse<Department>>(
      `${this.churchDepartments(churchId)}`,
      { params: this.paging(page, pageSize) },
    );
  }

  createDepartment(churchId: number, body: UpsertDepartmentRequest) {
    return this.http.post<Department>(this.churchDepartments(churchId), body);
  }

  updateDepartment(
    churchId: number,
    departmentId: number,
    body: UpsertDepartmentRequest,
  ) {
    return this.http.put<Department>(
      `${this.churchDepartments(churchId)}/${departmentId}`,
      body,
    );
  }

  deleteDepartment(churchId: number, departmentId: number) {
    return this.http.delete<void>(
      `${this.churchDepartments(churchId)}/${departmentId}`,
    );
  }

  assignDepartmentEmployee(
    churchId: number,
    departmentId: number,
    body: AddDepartmentEmployeeRequest,
  ) {
    return this.http.post<DepartmentEmployee>(
      `${this.churchDepartments(churchId)}/${departmentId}/employees`,
      body,
    );
  }

  getDepartmentEmployees(churchId: number, departmentId: number) {
    return this.http.get<DepartmentEmployee[]>(
      `${this.churchDepartments(churchId)}/${departmentId}/employees`,
    );
  }

  removeDepartmentEmployee(
    churchId: number,
    departmentId: number,
    departmentEmployeeId: number,
  ) {
    return this.http.delete<DepartmentEmployee>(
      `${this.churchDepartments(churchId)}/${departmentId}/employees/${departmentEmployeeId}`,
    );
  }

  // ---- Church accounts -------------------------------------------------

  getAccounts(districtId: number, churchId: number) {
    return this.http.get<UserAccount[]>(
      `${this.districtBase}/${districtId}/churches/${churchId}/accounts`,
    );
  }

  changeAccountRole(
    districtId: number,
    churchId: number,
    userId: string,
    body: RoleChangeRequest,
  ) {
    return this.http.post<void>(
      `${this.districtBase}/${districtId}/churches/${churchId}/accounts/${userId}/roles`,
      body,
    );
  }

  lockAccount(
    districtId: number,
    churchId: number,
    userId: string,
    body: LockAccountRequest,
  ) {
    return this.http.post<void>(
      `${this.districtBase}/${districtId}/churches/${churchId}/accounts/${userId}/lock`,
      body,
    );
  }

  unlockAccount(districtId: number, churchId: number, userId: string) {
    return this.http.post<void>(
      `${this.districtBase}/${districtId}/churches/${churchId}/accounts/${userId}/unlock`,
      null,
    );
  }

  // ---- URL helpers ------------------------------------------------------

  private churchEmployees(churchId: number): string {
    return `${environment.apiUrl}/churches/${churchId}/employees`;
  }

  private churchTeams(churchId: number): string {
    return `${environment.apiUrl}/churches/${churchId}/teams`;
  }

  private churchDepartments(churchId: number): string {
    return `${environment.apiUrl}/churches/${churchId}/departments`;
  }
}
