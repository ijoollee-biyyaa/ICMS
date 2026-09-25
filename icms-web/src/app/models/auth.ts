/** Mirrors LoginRequest — POST /api/auth/login. */
export interface LoginRequest {
  username: string;
  password: string;
}

/** Mirrors AuthResponse — POST /api/auth/login and /api/auth/refresh. */
export interface AuthResponse {
  accessToken: string;
}

/** Mirrors UserProfileDto — GET /api/auth/me. */
export interface UserProfile {
  userId: string;
  email: string;
  displayName: string;
  role: string;
  churchId: number | null;
  memberId: number | null;
  churchName: string | null;
}

/** Mirrors ChangePasswordRequest — POST /api/auth/change-password. */
export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}