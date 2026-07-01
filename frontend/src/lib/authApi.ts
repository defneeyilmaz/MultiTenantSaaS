import { apiClient } from '@/lib/api';
import type {
  AcceptInvitationResult,
  AuthTokens,
  CompanySignupResult,
} from '@/types/auth';

export async function login(
  email: string,
  password: string,
  tenantSlug: string,
): Promise<AuthTokens> {
  const response = await apiClient.post<AuthTokens>('/api/auth/login', {
    email,
    password,
    tenantSlug,
  });

  return response.data;
}

export async function companySignup(input: {
  companyName: string;
  adminEmail: string;
  adminPassword: string;
  companySlug?: string;
  adminFullName?: string;
}): Promise<CompanySignupResult> {
  const response = await apiClient.post<CompanySignupResult>('/api/auth/company-signup', input);
  return response.data;
}

export async function forgotPassword(email: string): Promise<void> {
  await apiClient.post('/api/auth/forgot-password', { email });
}

export async function resetPassword(input: {
  email: string;
  token: string;
  newPassword: string;
}): Promise<void> {
  await apiClient.post('/api/auth/reset-password', input);
}

export async function verifyEmail(email: string, token: string): Promise<void> {
  await apiClient.post('/api/auth/verify-email', {
    email: email.trim(),
    token: token.trim(),
  });
}

export async function acceptInvitation(input: {
  email: string;
  token: string;
  password: string;
  fullName?: string;
}): Promise<AcceptInvitationResult> {
  const response = await apiClient.post<AcceptInvitationResult>(
    '/api/users/accept-invitation',
    input,
  );

  return response.data;
}

export async function logout(): Promise<void> {
  const refreshToken = localStorage.getItem('refreshToken');
  if (refreshToken) {
    try {
      await apiClient.post('/api/auth/logout', { refreshToken });
    } catch {
      // Clear local session even if the server call fails.
    }
  }
}
