export type SalaryPaidBy = 'Church' | 'District';
export type EmploymentType =
  | 'FulltimeMinister'
  | 'DistrictStaff'
  | 'ChurchStaff'
  | 'Volunteer';
export type EmployeeStatus = 'Active' | 'OnLeave' | 'Resigned' | 'Terminated';
export type MinisterTitle =
  | 'Pastor'
  | 'Evangelist'
  | 'Prophet'
  | 'Teacher'
  | 'Apostle';

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

export interface LinkDto {
  href: string;
  rel: string;
  method: string;
}

/** List row from GET /api/districts — mirrors DistrictResponseDto. */
export interface DistrictSummary {
  id: number;
  name: string;
  code: string;
  address: string | null;
  createdAt: string;
  links?: LinkDto[];
}

/** Detail payload — mirrors DistrictDetailDto (includes church/member counts). */
export interface DistrictDetail extends DistrictSummary {
  churchCount: number;
  memberCount: number;
}

/** Body for POST /api/districts — camelCase JSON. */
export interface CreateDistrictRequest {
  name: string;
  code: string;
  address?: string | null;
}

/** Body for PUT /api/districts/{id}. */
export interface UpdateDistrictRequest {
  name: string;
  code: string;
  address?: string | null;
}

/**
 * One employee row — mirrors EmployeeResponseDto.
 * The payroll/account fields are populated for office hires that create accounts.
 */
export interface EmployeeRow {
  id: number;
  position: string;
  employmentType: EmploymentType;
  ministerTitle: MinisterTitle | null;
  memberId: number | null;
  memberName: string | null;
  memberEfgbcId: string | null;
  memberChurchId: number | null;
  memberChurchName: string | null;
  churchId: number | null;
  salary: number | null;
  salaryPaidBy: SalaryPaidBy;
  hireDate: string | null;
  status: EmployeeStatus;
  isDistrictPresident: boolean;
  isVicePresident: boolean;
  accountEmail?: string | null;
  accountTempPassword?: string | null;
}

/** Body for POST /api/districts/{id}/employees. */
export interface CreateEmployeeRequest {
  position?: string | null;
  employmentType?: EmploymentType | null;
  ministerTitle?: MinisterTitle | null;
  memberId?: number | null;
  salary?: number | null;
  hireDate?: string | null;
  isDistrictPresident?: boolean;
  isVicePresident?: boolean;
  firstName?: string | null;
  fatherName?: string | null;
  grandfatherName?: string | null;
}

/** Body for PUT /api/districts/{id}/employees/{employeeId}. */
export interface UpdateEmployeeRequest {
  position?: string | null;
  ministerTitle?: MinisterTitle | null;
  salary?: number | null;
  hireDate?: string | null;
  status?: EmployeeStatus | null;
  isDistrictPresident?: boolean;
  isVicePresident?: boolean;
}

export interface OfficeExecutives {
  president: EmployeeRow | null;
  vicePresident: EmployeeRow | null;
}

export interface MinisterPlacement {
  scope: string;
  churchId: number | null;
  position: string;
  isPresident: boolean;
  isVicePresident: boolean;
  salaryPaidBy: SalaryPaidBy;
}

export interface MinisterRow {
  memberId: number;
  fullName: string;
  efgbcId: string;
  homeChurchName: string | null;
  ministerTitle: MinisterTitle | null;
  isDistrictPaid: boolean;
  placements: MinisterPlacement[];
}
