export interface TenantUser {
  userId: string;
  email: string;
  fullName: string | null;
  role: string;
  isActive: boolean;
  joinedAt: string;
}

export interface Invitation {
  id: string;
  email: string;
  role: string;
  expiresAt: string;
}

export interface Role {
  id: string;
  name: string;
  description: string | null;
  permissions: string[];
}

export interface Permission {
  id: string;
  name: string;
  description: string | null;
}

export interface AuditLog {
  id: string;
  tenantId: string;
  actorUserId: string | null;
  actorEmail: string | null;
  action: string;
  entityType: string | null;
  entityId: string | null;
  details: string | null;
  ipAddress: string | null;
  createdAt: string;
}

export interface Tenant {
  id: string;
  name: string;
  slug: string;
  domain: string | null;
  isActive: boolean;
  createdAt: string;
}

export const MEMBERSHIP_ROLES = ['TenantAdmin', 'Manager', 'Employee'] as const;
export type MembershipRole = (typeof MEMBERSHIP_ROLES)[number];
