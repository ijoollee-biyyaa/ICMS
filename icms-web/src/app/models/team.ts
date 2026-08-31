import { LinkDto } from './district';

export type MembershipRule = 'Single' | 'Multiple';
export type TeamMemberRole = 'Member' | 'Leader';
export type TeamAttendanceStatus = 'Present' | 'Late' | 'Absent';

/** Mirrors TeamResponseDto from GET /api/churches/{id}/teams. */
export interface Team {
  id: number;
  churchId: number;
  parentTeamId: number | null;
  membershipRule: MembershipRule;
  name: string;
  createdAt: string;
  links?: LinkDto[];
}

/** Mirrors TeamDetailDto (adds sub-team and member counts). */
export interface TeamDetail {
  id: number;
  churchId: number;
  parentTeamId: number | null;
  membershipRule: MembershipRule;
  name: string;
  createdAt: string;
  subTeamCount: number;
  memberCount: number;
  links?: LinkDto[];
}

/** Body for POST /api/churches/{id}/teams. */
export interface CreateTeamRequest {
  name: string;
  membershipRule?: MembershipRule | null;
  parentTeamId?: number | null;
}

/** Body for PUT team (name only — parent and rule are fixed at creation). */
export interface UpdateTeamRequest {
  name: string;
}

/** Mirrors TeamMemberDto from GET teams/{id}/members. */
export interface TeamMember {
  memberId: number;
  memberName: string;
  memberEfgbcId: string;
  role: TeamMemberRole;
  joinedAt: string;
  links?: LinkDto[];
}

/** Body for POST teams/{id}/members. */
export interface JoinTeamRequest {
  memberId?: number | null;
}

/** Body for PUT teams/{id}/members/{memberId}/role. */
export interface SetRoleRequest {
  role?: TeamMemberRole | null;
}

/** Body for POST teams/{id}/attendance. */
export interface SaveAttendanceRequest {
  attendanceDate?: string | null;
  entries?: AttendanceEntryRequest[] | null;
}

export interface AttendanceEntryRequest {
  memberId?: number | null;
  status?: TeamAttendanceStatus | null;
  reason?: string | null;
}

/** Mirrors TeamAttendanceRecordDto. */
export interface TeamAttendanceRecord {
  memberId: number;
  memberName: string;
  memberEfgbcId: string;
  attendanceDate: string;
  status: TeamAttendanceStatus;
  reason: string | null;
}

/** Mirrors TeamAttendanceSummaryDto (per member, inside a report). */
export interface TeamAttendanceSummary {
  memberId: number;
  memberName: string;
  memberEfgbcId: string;
  totalMeetings: number;
  present: number;
  late: number;
  absent: number;
  attended: number;
  attendanceRate: number;
}

/** Mirrors TeamAttendanceReportDto from GET teams/{id}/attendance/summary. */
export interface TeamAttendanceReport {
  teamId: number;
  teamName: string;
  from: string | null;
  to: string | null;
  totalMeetings: number;
  totalPresent: number;
  totalLate: number;
  totalAbsent: number;
  overallRate: number;
  members: TeamAttendanceSummary[];
}

/** Mirrors SaveAttendanceResult returned by POST teams/{id}/attendance. */
export interface SaveAttendanceResult {
  attendanceDate: string;
  totalMarked: number;
  presentCount: number;
  lateCount: number;
  absentCount: number;
}

/** Body for POST teams/{id}/payments. Month is the first day of the month (yyyy-MM-dd). */
export interface RecordPaymentRequest {
  memberId?: number | null;
  month?: string | null;
  amount?: number | null;
}

/** Body for PUT teams/{id}/payments/{paymentId}. */
export interface UpdatePaymentRequest {
  amount?: number | null;
}

/** Mirrors TeamPaymentDto. */
export interface TeamPayment {
  id: number;
  memberId: number;
  memberName: string;
  memberEfgbcId: string;
  month: string;
  amount: number;
  paidAt: string;
  links?: LinkDto[];
}

/** Mirrors TeamPaymentSummaryDto from GET teams/{id}/payments/summary. */
export interface TeamPaymentSummary {
  memberId: number;
  memberName: string;
  memberEfgbcId: string;
  paymentCount: number;
  totalAmount: number;
  lastPaidMonth: string | null;
}