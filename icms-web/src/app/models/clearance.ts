export type ClearanceDirection = 'Incoming' | 'Outgoing';
export type ClearanceRequestStatus = 'Initiated' | 'Completed' | 'Voided';
export type ClearanceType = 'Internal' | 'CrossDistrict' | 'CrossDenomination';

export interface ClearanceRequest {
  id: number;
  memberId: number | null;
  memberName: string | null;
  memberEfgbcId: string | null;
  
  sourceChurchId: number | null;
  sourceChurchName: string | null;
  sourceDistrictOrDenomination: string | null;
  
  destinationChurchId: number | null;
  destinationChurchName: string | null;
  destinationDistrictOrDenomination: string | null;
  
  type: ClearanceType;
  direction: ClearanceDirection;
  status: ClearanceRequestStatus;
  
  clearanceCode: string;
  clearanceDocumentUrl: string | null;
  recommendationNotes: string | null;
  
  incomingFirstName: string | null;
  incomingFatherName: string | null;
  incomingGrandfatherName: string | null;
  previousEfgbcId: string | null;
  
  initiatedByUserId: string | null;
  initiatedAt: string;
  completedByUserId: string | null;
  completedAt: string | null;
  voidReason: string | null;
  voidedAt: string | null;
  createdAt: string;
}

export interface ClearanceStats {
  total: number;
  initiated: number;
  completed: number;
  voided: number;
  incoming: number;
  outgoing: number;
  internal: number;
  external: number;
}

export interface ClearanceMemberLookup {
  id: number;
  efgbcId: string;
  fullName: string;
  status: string;
  churchId: number;
  churchName: string;
}

export interface ClearanceChurchLookup {
  id: number;
  name: string;
  code: string;
  districtId: number;
  districtName: string;
}

export interface CreateIncomingClearanceRequest {
  destinationChurchId: number;
  sourceChurchName: string;
  sourceDistrictOrDenomination?: string;
  clearanceDocumentUrl?: string;
  incomingFirstName: string;
  incomingFatherName: string;
  incomingGrandfatherName: string;
  previousEfgbcId?: string;
}

export interface CreateOutgoingClearanceRequest {
  memberId: number;
  destinationChurchId?: number;
  destinationChurchName?: string;
  destinationDistrictOrDenomination?: string;
  type: ClearanceType;
  recommendationNotes?: string;
}

export interface AcceptTransferRequest {
  clearanceDocumentUrl?: string;
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}
