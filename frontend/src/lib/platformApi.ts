import { apiClient } from '@/lib/api';
import type { PlatformTenant } from '@/types/platform';

export async function listPlatformTenants(): Promise<PlatformTenant[]> {
  const response = await apiClient.get<PlatformTenant[]>('/api/platform/tenants');
  return response.data;
}

export async function createPlatformTenant(input: {
  name: string;
  slug?: string;
  domain?: string;
}): Promise<PlatformTenant> {
  const response = await apiClient.post<PlatformTenant>('/api/platform/tenants', {
    name: input.name,
    slug: input.slug?.trim() ? input.slug.trim() : null,
    domain: input.domain?.trim() ? input.domain.trim() : null,
  });
  return response.data;
}

export async function updatePlatformTenant(
  tenantId: string,
  input: {
    name?: string;
    domain?: string;
    isActive?: boolean;
  },
): Promise<PlatformTenant> {
  const response = await apiClient.patch<PlatformTenant>(`/api/platform/tenants/${tenantId}`, input);
  return response.data;
}
