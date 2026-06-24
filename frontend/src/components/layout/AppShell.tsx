import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { logout } from '@/lib/authApi';
import {
  clearAuthTokens,
  getStoredEmail,
  getStoredTenantSlug,
  isAuthenticated,
} from '@/lib/authStorage';
import { SecondaryButton } from '@/components/ui/FormField';

const navItems = [
  { to: '/', label: 'Home', end: true },
  { to: '/app/admin', label: 'Admin' },
];

export function AppShell() {
  const navigate = useNavigate();
  const signedIn = isAuthenticated();

  async function handleLogout() {
    await logout();
    clearAuthTokens();
    navigate('/login');
  }

  return (
    <div className="min-h-screen bg-slate-950 text-slate-100">
      <header className="border-b border-slate-800 bg-slate-900/80 backdrop-blur">
        <div className="mx-auto flex max-w-6xl items-center justify-between gap-4 px-4 py-4">
          <div>
            <p className="text-xs font-semibold uppercase tracking-[0.2em] text-sky-400">
              MultiTenantSaaS
            </p>
            <h1 className="text-lg font-semibold">Workspace Console</h1>
          </div>

          <div className="flex flex-wrap items-center gap-3">
            {signedIn ? (
              <p className="hidden text-sm text-slate-400 sm:block">
                {getStoredEmail()} · {getStoredTenantSlug()}
              </p>
            ) : null}

            <nav className="flex items-center gap-2">
              {navItems.map((item) => (
                <NavLink
                  key={item.to}
                  to={item.to}
                  end={item.end}
                  className={({ isActive }) =>
                    [
                      'rounded-lg px-3 py-2 text-sm font-medium transition',
                      isActive
                        ? 'bg-sky-500/15 text-sky-300'
                        : 'text-slate-400 hover:bg-slate-800 hover:text-slate-100',
                    ].join(' ')
                  }
                >
                  {item.label}
                </NavLink>
              ))}
            </nav>

            {signedIn ? (
              <SecondaryButton type="button" onClick={() => void handleLogout()}>
                Sign out
              </SecondaryButton>
            ) : (
              <NavLink
                to="/login"
                className="rounded-lg bg-sky-500 px-3 py-2 text-sm font-medium text-slate-950 transition hover:bg-sky-400"
              >
                Sign in
              </NavLink>
            )}
          </div>
        </div>
      </header>

      <main className="mx-auto max-w-6xl px-4 py-8">
        <Outlet />
      </main>
    </div>
  );
}
