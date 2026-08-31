import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

import { environment } from '../../environments/environment';
import {
  AuthResponse,
  ChangePasswordRequest,
  LoginRequest,
  UserProfile,
} from '../models/auth';

const ROLE_CLAIM_URI =
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

type JwtPayload = Record<string, string | string[] | undefined>;

/**
 * Client for the cookie+JWT auth flow:
 * - Access token: in-memory (15 min), attached as `Authorization: Bearer`.
 * - Refresh token: HttpOnly `icms_refresh` cookie — invisible to JS, sent by the
 *   browser automatically thanks to `withCredentials`.
 * - The canonical profile comes from GET /api/auth/me; the full role list is
 *   decoded from the JWT because /me reports only a single role.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/auth`;

  readonly accessToken = signal<string | null>(null);
  readonly currentUser = signal<UserProfile | null>(null);

  /** Every role carried by the JWT (the backend /me only exposes one). */
  private readonly decodedRoles = signal<string[]>([]);
  readonly roles = computed(() => this.decodedRoles());

  readonly isAuthenticated = computed(() => this.accessToken() !== null);

  readonly memberId = computed(() => this.currentUser()?.memberId ?? null);

  hasRole(role: string): boolean {
    const roles = this.decodedRoles();
    return roles.includes(role) || roles.includes('Admin');
  }

  hasAnyRole(roles: string[]): boolean {
    return roles.some((role) => this.hasRole(role));
  }

  canManageChurch(): boolean {
    return this.hasRole('ChurchAdmin');
  }

  homes(): string[] {
    const roles = this.decodedRoles();
    const homes: string[] = [];
    if (roles.includes('Admin') || roles.includes('DistrictSubAdmin')) {
      homes.push('/district');
    }
    if (roles.includes('ChurchAdmin')) {
      homes.push('/church');
    }
    if (roles.includes('Member')) {
      homes.push('/member');
    }
    return homes;
  }

  getAccessToken(): string | null {
    return this.accessToken();
  }

  async login(credentials: LoginRequest): Promise<UserProfile> {
    const res = await firstValueFrom(
      this.http.post<AuthResponse>(`${this.base}/login`, credentials),
    );
    await this.applySession(res.accessToken);
    return this.currentUser()!;
  }

  /**
   * Restores the session from the HttpOnly refresh cookie. Returns true when a
   * fresh access token was issued. Called at app boot and by the auth guard.
   */
  async refresh(): Promise<boolean> {
    try {
      const res = await firstValueFrom(
        this.http.post<AuthResponse>(`${this.base}/refresh`, null),
      );
      await this.applySession(res.accessToken);
      return true;
    } catch {
      this.clearSession();
      return false;
    }
  }

  async logout(): Promise<void> {
    try {
      await firstValueFrom(this.http.post<void>(`${this.base}/logout`, null));
    } catch {
      // ignore network errors; local session is cleared regardless
    }
    this.clearSession();
  }

  /**
   * Changes the current user's password. The backend revokes every refresh
   * token and clears the cookie, so the client session is cleared too.
   */
  async changePassword(
    currentPassword: string,
    newPassword: string,
  ): Promise<void> {
    const body: ChangePasswordRequest = { currentPassword, newPassword };
    await firstValueFrom(
      this.http.post<void>(`${this.base}/change-password`, body),
    );
    this.clearSession();
  }

  clearSession() {
    this.accessToken.set(null);
    this.decodedRoles.set([]);
    this.currentUser.set(null);
  }

  // ---- session wiring ----------------------------------------------------

  private async applySession(accessToken: string): Promise<void> {
    this.accessToken.set(accessToken);
    this.decodedRoles.set(this.decodeRoles(accessToken));
    this.currentUser.set(await this.fetchProfileOrFallback(accessToken));
  }

  /**
   * The server is the source of truth for the profile; the local JWT decode is
   * only a fallback (e.g. /me down) so a valid login never strands the user.
   */
  private async fetchProfileOrFallback(token: string): Promise<UserProfile> {
    try {
      return await firstValueFrom(
        this.http.get<UserProfile>(`${this.base}/me`),
      );
    } catch {
      return this.decodeProfile(token);
    }
  }

  private decodePayload(token: string): JwtPayload {
    try {
      return JSON.parse(
        atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')),
      );
    } catch {
      return {};
    }
  }

  private decodeRoles(token: string): string[] {
    const payload = this.decodePayload(token);
    const raw = payload[ROLE_CLAIM_URI] ?? payload['role'] ?? '';
    const roles = Array.isArray(raw)
      ? raw
      : raw
        ? [raw]
        : [];
    return roles;
  }

  private decodeProfile(token: string): UserProfile {
    const payload = this.decodePayload(token);
    const roles = this.decodeRoles(token);
    const str = (key: string): string => {
      const value = payload[key];
      return Array.isArray(value) ? (value[0] ?? '') : (value ?? '');
    };
    const userId = str('sub') || str('nameid') || str('userId');
    const email = str('email') || str('upn');
    return {
      userId,
      email,
      displayName:
        [str('FirstName'), str('FatherName'), str('GrandfatherName')]
          .filter(Boolean)
          .join(' ') || email || 'User',
      role: roles.includes('Admin') ? 'Admin' : (roles[0] ?? ''),
      churchId: str('ChurchId') ? Number(str('ChurchId')) : null,
      memberId: str('MemberId') ? Number(str('MemberId')) : null,
    };
  }
}