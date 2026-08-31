export type Gender = 'Male' | 'Female';
export type JobStatus =
  | 'Student'
  | 'Employed'
  | 'SelfEmployed'
  | 'Unemployed'
  | 'Retired'
  | 'Other';
export type MemberStatus = 'Active' | 'Transferring' | 'Deactivated' | 'Archived';
export type JoinChannel = 'Baptism' | 'Salvation' | 'Transfer' | 'Return';

/** Mirrors MemberResponseDto from GET /api/members. */
export interface Member {
  id: number;
  efgbcId: string;
  churchId: number;
  firstName: string;
  fatherName: string;
  grandfatherName: string;
  dateOfBirth: string | null;
  gender: Gender;
  jobStatus: JobStatus;
  phone: string | null;
  email: string | null;
  photoUrl: string | null;
  status: MemberStatus;
  joinedVia: JoinChannel;
  joinedAt: string | null;
  createdAt: string;
}

/** Body for POST /api/members — mirrors CreateMemberRequest. */
export interface CreateMemberRequest {
  churchId: number;
  firstName: string;
  fatherName: string;
  grandfatherName: string;
  dateOfBirth: string | null;
  gender: Gender;
  jobStatus: JobStatus;
  phone: string | null;
  email: string | null;
  photoUrl: string | null;
  joinedVia: JoinChannel;
  joinedAt: string | null;
}

/** Body for PUT /api/members/{id} — mirrors UpdateMemberRequest. */
export interface UpdateMemberRequest {
  firstName: string;
  fatherName: string;
  grandfatherName: string;
  dateOfBirth: string | null;
  gender: Gender;
  jobStatus: JobStatus;
  phone: string | null;
  email: string | null;
  photoUrl: string | null;
}

// ---- Member dashboard (GET /api/members/{id}/dashboard) ----------------

/** Member service assignment inside a member's dashboard. */
export interface MemberService {
  employeeId: number;
  scope: string;
  churchId: number | null;
  position: string | null;
  employmentType: import('./district').EmploymentType | null;
  isPresident: boolean;
  isVicePresident: boolean;
  departments: MemberDepartmentAssignment[];
}

export interface MemberDepartmentAssignment {
  departmentId: number;
  departmentName: string;
  role: import('./department').DepartmentEmployeeRole;
}

export interface MemberProfile {
  fullName: string;
  efgbcId: string;
  churchName: string;
  dateOfBirth: string | null;
  age: number | null;
  gender: Gender;
  phone: string | null;
  email: string | null;
  status: MemberStatus;
  joinedAt: string | null;
}

export interface MemberAttendanceSummary {
  meetings: number;
  present: number;
  late: number;
  absent: number;
  attended: number;
  attendanceRate: number | null;
  lastAttendanceDate: string | null;
}

export interface MemberPaymentSummary {
  paymentCount: number;
  totalAmount: number;
  lastPaidMonth: string | null;
}

export interface MemberTeamCard {
  teamId: number;
  teamName: string;
  role: string;
  attendance: MemberAttendanceSummary;
  payments: MemberPaymentSummary;
}

export interface MemberServiceSummary {
  meetingCount: number;
  attendedCount: number;
  attendanceRate: number | null;
  totalPayments: number;
  services: MemberService[];
}

/** Mirrors MemberDashboardDto. */
export interface MemberDashboard {
  memberId: number;
  profile: MemberProfile;
  teamCount: number;
  teams: MemberTeamCard[];
  service: MemberServiceSummary;
}

// ---- Member stats & login accounts -------------------------------------

/** Mirrors MemberStatsDto — GET /api/members/stats. */
export interface MemberStats {
  total: number;
  active: number;
  transferring: number;
  inactive: number;
}

/** Mirrors MemberAccountInfoDto — GET /api/members/{id}/account. */
export interface MemberAccountInfo {
  memberId: number;
  hasAccount: boolean;
  email: string | null;
}

/** Mirrors IssueMemberCredentialsDto — POST /api/members/{id}/account. */
export interface IssueMemberCredentials {
  memberId: number;
  email: string;
  /** Only present when the account was just created; shown once. */
  tempPassword: string | null;
}
