import axios from 'axios';
import { apiConfig } from './config';

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

export async function checkApiHealth(): Promise<boolean> {
  try {
    const response = await apiClient.get('/health');
    return response.status === 200;
  } catch {
    return false;
  }
}
