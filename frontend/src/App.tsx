import { Navigate, Route, Routes } from 'react-router-dom';
import { AuthLayout } from '@/components/layout/AuthLayout';
import { AppShell } from '@/components/layout/AppShell';
import { AppAreaPage } from '@/pages/AppAreaPage';
import { HomePage } from '@/pages/HomePage';
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
        <Route path="app" element={<AppAreaPage />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  );
}
