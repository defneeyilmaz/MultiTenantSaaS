export interface AuthTokens {
  accessToken: string;
  accessTokenExpiresAt: string;
  refreshToken: string;
  refreshTokenExpiresAt: string;
  tenantId: string;
  tenantSlug: string;
  userId: string;
  email: string;
  role: string;
}

export interface CompanySignupResult {
  tenantId: string;
  tenantSlug: string;
  userId: string;
  adminEmail: string;
  role: string;
}

export interface AcceptInvitationResult {
  userId: string;
  email: string;
  tenantId: string;
  tenantSlug: string;
  role: string;
}
