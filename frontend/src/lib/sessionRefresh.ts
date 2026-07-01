import axios from 'axios';
import { apiConfig } from '@/lib/config';
import { clearAuthTokens, saveAuthTokens } from '@/lib/authStorage';
import type { AuthTokens } from '@/types/auth';

const refreshClient = axios.create({
  baseURL: apiConfig.baseUrl,
  headers: {
    'Content-Type': 'application/json',
  },
});

let refreshPromise: Promise<AuthTokens | null> | null = null;

export async function refreshSession(): Promise<AuthTokens | null> {
  const refreshToken = localStorage.getItem('refreshToken');
  if (!refreshToken) {
    return null;
  }

  if (!refreshPromise) {
    refreshPromise = refreshClient
      .post<AuthTokens>('/api/auth/refresh-token', { refreshToken })
      .then((response) => {
        saveAuthTokens(response.data);
        return response.data;
      })
      .catch(() => {
        clearAuthTokens();
        return null;
      })
      .finally(() => {
        refreshPromise = null;
      });
  }

  return refreshPromise;
}

export function redirectToLogin(): void {
  if (typeof window === 'undefined') {
    return;
  }

  const path = window.location.pathname;
  if (path.startsWith('/login') || path.startsWith('/signup')) {
    return;
  }

  window.location.href = '/login';
}
