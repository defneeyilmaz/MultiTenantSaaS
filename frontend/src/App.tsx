import { Navigate, Route, Routes } from 'react-router-dom';
import { AppShell } from '@/components/layout/AppShell';
import { AppAreaPage } from '@/pages/AppAreaPage';
import { AuthShellPage } from '@/pages/AuthShellPage';
import { HomePage } from '@/pages/HomePage';

export default function App() {
  return (
    <Routes>
      <Route element={<AppShell />}>
        <Route index element={<HomePage />} />
        <Route path="auth" element={<AuthShellPage />} />
        <Route path="app" element={<AppAreaPage />} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  );
}
