import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { checkApiHealth } from '@/lib/api';

export function HomePage() {
  const [apiHealthy, setApiHealthy] = useState<boolean | null>(null);

  useEffect(() => {
    void checkApiHealth().then(setApiHealthy);
  }, []);

  return (
    <section className="space-y-6">
      <div className="rounded-2xl border border-slate-800 bg-slate-900/60 p-8">
        <p className="text-sm font-medium text-sky-400">Phase 6 / #24</p>
        <h2 className="mt-2 text-3xl font-semibold tracking-tight">
          Public auth pages
        </h2>
        <p className="mt-3 max-w-2xl text-slate-400">
          Login, signup, password reset, and invitation acceptance are wired to the API.
          Tenant admin and user workspace screens come next.
        </p>
      </div>

      <div className="grid gap-4 md:grid-cols-3">
        <StatusCard
          title="API health"
          value={
            apiHealthy === null
              ? 'Checking...'
              : apiHealthy
                ? 'Healthy'
                : 'Unavailable'
          }
          tone={apiHealthy ? 'success' : apiHealthy === false ? 'danger' : 'neutral'}
        />
        <StatusCard title="Router" value="React Router" tone="success" />
        <StatusCard title="HTTP client" value="Axios" tone="success" />
      </div>

      <div className="flex flex-wrap gap-3">
        <Link
          to="/login"
          className="rounded-lg bg-sky-500 px-4 py-2 text-sm font-medium text-slate-950 transition hover:bg-sky-400"
        >
          Sign in
        </Link>
        <Link
          to="/signup"
          className="rounded-lg border border-slate-700 px-4 py-2 text-sm font-medium text-slate-200 transition hover:border-slate-500 hover:bg-slate-900"
        >
          Create workspace
        </Link>
        <Link
          to="/app"
          className="rounded-lg border border-slate-700 px-4 py-2 text-sm font-medium text-slate-200 transition hover:border-slate-500 hover:bg-slate-900"
        >
          App area
        </Link>
      </div>
    </section>
  );
}

function StatusCard({
  title,
  value,
  tone,
}: {
  title: string;
  value: string;
  tone: 'success' | 'danger' | 'neutral';
}) {
  const toneClasses = {
    success: 'text-emerald-400',
    danger: 'text-rose-400',
    neutral: 'text-slate-300',
  }[tone];

  return (
    <div className="rounded-2xl border border-slate-800 bg-slate-900/40 p-5">
      <p className="text-sm text-slate-500">{title}</p>
      <p className={`mt-2 text-lg font-semibold ${toneClasses}`}>{value}</p>
    </div>
  );
}
