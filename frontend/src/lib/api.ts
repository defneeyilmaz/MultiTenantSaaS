import axios from 'axios';
import { apiConfig } from './config';
import { redirectToLogin, refreshSession } from './sessionRefresh';

export const apiClient = axios.create({
  baseURL: apiConfig.baseUrl,
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.request.use((config) => {
  const accessToken = localStorage.getItem('accessToken');
  const tenantSlug = localStorage.getItem('tenantSlug');

  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`;
  }

  if (tenantSlug) {
    config.headers[apiConfig.tenantSlugHeader] = tenantSlug;
  }

  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  async (error: unknown) => {
    if (!axios.isAxiosError(error) || !error.config) {
      return Promise.reject(error);
    }

    const originalRequest = error.config as typeof error.config & { _retry?: boolean };
    const status = error.response?.status;
    const url = originalRequest.url ?? '';

    if (status !== 401 || originalRequest._retry || url.includes('/api/auth/')) {
      return Promise.reject(error);
    }

    originalRequest._retry = true;

    const tokens = await refreshSession();
    if (!tokens) {
      redirectToLogin();
      return Promise.reject(error);
    }

    originalRequest.headers.Authorization = `Bearer ${tokens.accessToken}`;
    return apiClient(originalRequest);
  },
);

export async function checkApiHealth(): Promise<boolean> {
  try {
    const response = await apiClient.get('/health');
    return response.status === 200;
  } catch {
    return false;
  }
}
