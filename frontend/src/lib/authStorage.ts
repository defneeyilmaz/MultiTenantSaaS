import type { AuthTokens } from '@/types/auth';

const keys = {
  accessToken: 'accessToken',
  refreshToken: 'refreshToken',
  tenantSlug: 'tenantSlug',
  email: 'email',
  role: 'role',
} as const;

export function saveAuthTokens(tokens: AuthTokens): void {
  localStorage.setItem(keys.accessToken, tokens.accessToken);
  localStorage.setItem(keys.refreshToken, tokens.refreshToken);
  localStorage.setItem(keys.tenantSlug, tokens.tenantSlug);
  localStorage.setItem(keys.email, tokens.email);
  localStorage.setItem(keys.role, tokens.role);
}

export function clearAuthTokens(): void {
  Object.values(keys).forEach((key) => localStorage.removeItem(key));
}

export function isAuthenticated(): boolean {
  return Boolean(localStorage.getItem(keys.accessToken));
}
