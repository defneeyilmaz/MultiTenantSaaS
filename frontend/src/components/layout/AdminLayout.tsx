import { NavLink, Outlet } from 'react-router-dom';
import { hasPermission } from '@/lib/permissions';

const navItems = [
  { to: '/app/admin', label: 'Overview', end: true, permission: null },
  { to: '/app/admin/users', label: 'Users', permission: 'users.view' },
  { to: '/app/admin/roles', label: 'Roles', permission: 'roles.view' },
  { to: '/app/admin/audit', label: 'Audit log', permission: 'audit.view' },
  { to: '/app/admin/settings', label: 'Settings', permission: 'settings.manage' },
] as const;

export function AdminLayout() {
  const visibleItems = navItems.filter(
    (item) => !item.permission || hasPermission(item.permission),
  );

  return (
    <div className="grid gap-6 lg:grid-cols-[220px_1fr]">
      <aside className="rounded-2xl border border-slate-800 bg-slate-900/40 p-4">
        <p className="mb-3 text-xs font-semibold uppercase tracking-[0.2em] text-slate-500">
          Tenant admin
        </p>
        <nav className="space-y-1">
          {visibleItems.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={'end' in item ? item.end : false}
              className={({ isActive }) =>
                [
                  'block rounded-lg px-3 py-2 text-sm font-medium transition',
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
      </aside>

      <div>
        <Outlet />
      </div>
    </div>
  );
}
