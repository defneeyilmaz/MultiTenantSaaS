const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? '';

export const apiConfig = {
  baseUrl: API_BASE_URL,
  tenantSlugHeader: 'X-Tenant-Slug',
} as const;
