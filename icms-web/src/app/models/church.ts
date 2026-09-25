import { Member } from './member';

export type ChurchType = 'Local' | 'Daughter';

/** Mirrors ChurchResponseDto from GET /api/districts/{id}/churches. */
export interface Church {
  id: number;
  name: string;
  type: ChurchType;
  code: string;
  parentChurchId: number | null;
  city: string | null;
  subcity: string | null;
  email: string | null;
  phone: string | null;
  tel: string | null;
  mapAddress: string | null;
  websiteUrl: string | null;
  memberCount?: number;
  employeeCount?: number;
  ministerCount?: number;
}

/** Mirrors ChurchDetailDto (adds daughter/member/employee counts + HATEOAS links). */
export interface ChurchDetail extends Church {
  daughterCount: number;
  memberCount: number;
  activeMemberCount: number;
  employeeCount: number;
  ministerCount: number;
}

/** Body for POST /api/districts/{id}/churches — mirrors CreateChurchRequest. */
export interface CreateChurchRequest {
  name: string;
  type: ChurchType;
  code: string;
  parentChurchId: number | null;
  city: string | null;
  subcity: string | null;
  email: string | null;
  phone: string | null;
  tel: string | null;
  mapAddress: string | null;
  websiteUrl: string | null;
  adminFirstName: string;
  adminFatherName: string;
  adminGrandfatherName: string;
}

/** Body for PUT /api/districts/{id}/churches/{churchId} — mirrors UpdateChurchRequest. */
export interface UpdateChurchRequest {
  name: string;
  code: string;
  city: string | null;
  subcity: string | null;
  email: string | null;
  phone: string | null;
  tel: string | null;
  mapAddress: string | null;
  websiteUrl: string | null;
}

/** Mirrors CreateChurchResponseDto (church + one-time admin credentials). */
export interface CreateChurchResponse {
  church: Church;
  adminEmail: string;
  adminTempPassword: string;
}

export type { Member };
