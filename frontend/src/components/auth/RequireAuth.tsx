import { Navigate, Outlet } from 'react-router-dom';
import { isAuthenticated } from '@/lib/authStorage';

export function RequireAuth() {
  if (!isAuthenticated()) {
    return <Navigate to="/login" replace />;
  }

  return <Outlet />;
}
