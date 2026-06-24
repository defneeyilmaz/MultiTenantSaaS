import { Navigate, Route, Routes } from 'react-router-dom';
import { RequireAuth } from '@/components/auth/RequireAuth';
import { AdminLayout } from '@/components/layout/AdminLayout';
import { AuthLayout } from '@/components/layout/AuthLayout';
import { AppShell } from '@/components/layout/AppShell';
import { HomePage } from '@/pages/HomePage';
import { AdminOverviewPage } from '@/pages/admin/AdminOverviewPage';
import { AuditLogsPage } from '@/pages/admin/AuditLogsPage';
import { RolesPage } from '@/pages/admin/RolesPage';
import { SettingsPage } from '@/pages/admin/SettingsPage';
import { UsersPage } from '@/pages/admin/UsersPage';
import { AcceptInvitationPage } from '@/pages/auth/AcceptInvitationPage';
import { ForgotPasswordPage } from '@/pages/auth/ForgotPasswordPage';
import { LoginPage } from '@/pages/auth/LoginPage';
import { ResetPasswordPage } from '@/pages/auth/ResetPasswordPage';
import { SignupPage } from '@/pages/auth/SignupPage';

export default function App() {
  return (
    <Routes>
      <Route element={<AuthLayout />}>
        <Route path="login" element={<LoginPage />} />
        <Route path="signup" element={<SignupPage />} />
        <Route path="forgot-password" element={<ForgotPasswordPage />} />
        <Route path="reset-password" element={<ResetPasswordPage />} />
        <Route path="accept-invitation" element={<AcceptInvitationPage />} />
      </Route>

      <Route element={<AppShell />}>
        <Route index element={<HomePage />} />
        <Route path="app" element={<RequireAuth />}>
          <Route index element={<Navigate to="/app/admin" replace />} />
          <Route path="admin" element={<AdminLayout />}>
            <Route index element={<AdminOverviewPage />} />
            <Route path="users" element={<UsersPage />} />
            <Route path="roles" element={<RolesPage />} />
            <Route path="audit" element={<AuditLogsPage />} />
            <Route path="settings" element={<SettingsPage />} />
          </Route>
        </Route>
        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  );
}
