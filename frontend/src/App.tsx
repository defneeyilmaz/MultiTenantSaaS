import { Navigate, Route, Routes } from 'react-router-dom';
import { RequireAuth } from '@/components/auth/RequireAuth';
import { AdminLayout } from '@/components/layout/AdminLayout';
import { AuthLayout } from '@/components/layout/AuthLayout';
import { AppShell } from '@/components/layout/AppShell';
import { WorkspaceLayout } from '@/components/layout/WorkspaceLayout';
import { PlatformLayout } from '@/components/layout/PlatformLayout';
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
import { VerifyEmailPage } from '@/pages/auth/VerifyEmailPage';
import { ProfilePage } from '@/pages/workspace/ProfilePage';
import { ProjectsPage } from '@/pages/workspace/ProjectsPage';
import { TasksPage } from '@/pages/workspace/TasksPage';
import { WorkspaceOverviewPage } from '@/pages/workspace/WorkspaceOverviewPage';
import { PlatformOverviewPage } from '@/pages/platform/PlatformOverviewPage';
import { TenantsPage } from '@/pages/platform/TenantsPage';

export default function App() {
  return (
    <Routes>
      <Route element={<AuthLayout />}>
        <Route path="login" element={<LoginPage />} />
        <Route path="signup" element={<SignupPage />} />
        <Route path="forgot-password" element={<ForgotPasswordPage />} />
        <Route path="reset-password" element={<ResetPasswordPage />} />
        <Route path="verify-email" element={<VerifyEmailPage />} />
        <Route path="accept-invitation" element={<AcceptInvitationPage />} />
      </Route>

      <Route element={<AppShell />}>
        <Route index element={<HomePage />} />
        <Route path="app" element={<RequireAuth />}>
          <Route index element={<Navigate to="/app/workspace" replace />} />
          <Route path="workspace" element={<WorkspaceLayout />}>
            <Route index element={<WorkspaceOverviewPage />} />
            <Route path="projects" element={<ProjectsPage />} />
            <Route path="tasks" element={<TasksPage />} />
            <Route path="profile" element={<ProfilePage />} />
          </Route>
          <Route path="admin" element={<AdminLayout />}>
            <Route index element={<AdminOverviewPage />} />
            <Route path="users" element={<UsersPage />} />
            <Route path="roles" element={<RolesPage />} />
            <Route path="audit" element={<AuditLogsPage />} />
            <Route path="settings" element={<SettingsPage />} />
          </Route>
          <Route path="platform" element={<PlatformLayout />}>
            <Route index element={<PlatformOverviewPage />} />
            <Route path="tenants" element={<TenantsPage />} />
          </Route>
        </Route>
        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  );
}
