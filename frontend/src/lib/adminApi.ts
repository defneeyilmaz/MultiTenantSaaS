import { apiClient } from '@/lib/api';
import type {
  AuditLog,
  Invitation,
  Permission,
  Role,
  Tenant,
  TenantUser,
} from '@/types/admin';

export async function listUsers(): Promise<TenantUser[]> {
  const response = await apiClient.get<TenantUser[]>('/api/users');
  return response.data;
}

export async function inviteUser(email: string, role: string): Promise<Invitation> {
  const response = await apiClient.post<Invitation>('/api/users/invite', { email, role });
  return response.data;
}

export async function assignUserRole(userId: string, role: string): Promise<TenantUser> {
  const response = await apiClient.patch<TenantUser>(`/api/users/${userId}/role`, { role });
  return response.data;
}

export async function disableUser(userId: string): Promise<TenantUser> {
  const response = await apiClient.patch<TenantUser>(`/api/users/${userId}/disable`);
  return response.data;
}

export async function listRoles(): Promise<Role[]> {
  const response = await apiClient.get<Role[]>('/api/roles');
  return response.data;
}

export async function createRole(name: string, description?: string): Promise<Role> {
  const response = await apiClient.post<Role>('/api/roles', { name, description });
  return response.data;
}

export async function listPermissions(): Promise<Permission[]> {
  const response = await apiClient.get<Permission[]>('/api/permissions');
  return response.data;
}

export async function updateRolePermissions(
  roleId: string,
  permissionNames: string[],
): Promise<Role> {
  const response = await apiClient.put<Role>(`/api/roles/${roleId}/permissions`, {
    permissionNames,
  });
  return response.data;
}

export async function listAuditLogs(): Promise<AuditLog[]> {
  const response = await apiClient.get<AuditLog[]>('/api/audit-logs');
  return response.data;
}

export async function getCurrentTenant(): Promise<Tenant> {
  const response = await apiClient.get<Tenant>('/api/tenants/current');
  return response.data;
}

export async function updateTenantSettings(name: string, domain?: string): Promise<Tenant> {
  const response = await apiClient.put<Tenant>('/api/tenants/current/settings', {
    name,
    domain: domain?.trim() ? domain.trim() : null,
  });
  return response.data;
}
