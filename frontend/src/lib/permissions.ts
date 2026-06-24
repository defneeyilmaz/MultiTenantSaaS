const PERMISSION_CLAIM = 'permissions';

function decodeJwtPayload(token: string): Record<string, unknown> | null {
  try {
    const segment = token.split('.')[1];
    if (!segment) {
      return null;
    }

    const normalized = segment.replace(/-/g, '+').replace(/_/g, '/');
    const json = atob(normalized);
    return JSON.parse(json) as Record<string, unknown>;
  } catch {
    return null;
  }
}

function normalizePermissionClaim(value: unknown): string[] {
  if (Array.isArray(value)) {
    return value.filter((item): item is string => typeof item === 'string');
  }

  if (typeof value === 'string') {
    return [value];
  }

  return [];
}

export function getPermissionsFromToken(token: string | null): string[] {
  if (!token) {
    return [];
  }

  const payload = decodeJwtPayload(token);
  if (!payload) {
    return [];
  }

  return normalizePermissionClaim(payload[PERMISSION_CLAIM]);
}

export function getStoredPermissions(): string[] {
  return getPermissionsFromToken(localStorage.getItem('accessToken'));
}

export function hasPermission(permission: string): boolean {
  return getStoredPermissions().includes(permission);
}

export function hasAnyPermission(permissions: string[]): boolean {
  const stored = new Set(getStoredPermissions());
  return permissions.some((permission) => stored.has(permission));
}
