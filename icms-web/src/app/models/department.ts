export type DepartmentType =
  | 'Spiritual'
  | 'Charity'
  | 'Development'
  | 'Administrative';

export type DepartmentEmployeeRole = 'Head' | 'Staff' | 'Secretary';

/** Mirrors DepartmentResponseDto. */
export interface Department {
  id: number;
  name: string;
  type: DepartmentType;
  headEmployeeId: number | null;
  headEmployeeName: string | null;
  employeeCount: number;
}

/** Body for POST/PUT department — mirrors CreateDepartmentRequest / UpdateDepartmentRequest. */
export interface UpsertDepartmentRequest {
  name?: string | null;
  type?: DepartmentType | null;
  headEmployeeId?: number | null;
}

/** Mirrors DepartmentEmployeeDto (assigned employee on a department). */
export interface DepartmentEmployee {
  id: number;
  employeeId: number;
  employeeName: string | null;
  employeePosition: string;
  role: DepartmentEmployeeRole;
}

/** Body for POST department/{id}/employees — mirrors AddDepartmentEmployeeRequest. */
export interface AddDepartmentEmployeeRequest {
  employeeId?: number | null;
  role?: DepartmentEmployeeRole | null;
}
