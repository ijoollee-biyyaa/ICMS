/** Why an account was locked. Mirrors AccountLockReason (enum name as string). */
export type AccountLockReason = 'None' | 'ClearanceOut' | 'Death' | 'BySystem';

/** Mirrors AccountKind (enum name as string). */
export type AccountKind = 'Employee' | 'Member';

/** Mirrors UserAccountDto from GET /api/districts/{id}/accounts (also church accounts). */
export interface UserAccount {
  userId: string;
  email: string;
  firstName: string;
  fatherName: string;
  grandfatherName: string;
  position: string | null;
  churchId: number | null;
  districtId: number | null;
  roles: string[];
  isAccountLocked: boolean;
  lockReason: AccountLockReason;
  kind: AccountKind;
}

/** Body for POST accounts/{userId}/roles — mirrors RoleChangeRequest. Grant or revoke a role. */
export interface RoleChangeRequest {
  role: string;
  grant: boolean;
}

/** Body for POST accounts/{userId}/lock — mirrors LockAccountRequest. */
export interface LockAccountRequest {
  reason: AccountLockReason;
}
