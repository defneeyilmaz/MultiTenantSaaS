import { Link, Outlet } from 'react-router-dom';

export function AuthLayout() {
  return (
    <div className="min-h-screen bg-slate-950 text-slate-100">
      <div className="mx-auto flex min-h-screen max-w-lg flex-col justify-center px-4 py-10">
        <div className="mb-8 text-center">
          <Link to="/" className="inline-block">
            <p className="text-xs font-semibold uppercase tracking-[0.2em] text-sky-400">
              MultiTenantSaaS
            </p>
            <h1 className="mt-2 text-2xl font-semibold">Workspace access</h1>
          </Link>
        </div>

        <Outlet />

        <p className="mt-8 text-center text-sm text-slate-500">
          <Link to="/" className="hover:text-slate-300">
            Back to home
          </Link>
        </p>
      </div>
    </div>
  );
}
