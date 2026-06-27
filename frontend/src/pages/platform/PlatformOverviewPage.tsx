import { Link } from 'react-router-dom';
import { PageHeader } from '@/components/ui/PageHeader';
import { SectionCard } from '@/components/ui/SectionCard';
import { hasPermission } from '@/lib/permissions';
import { getStoredEmail, getStoredRole } from '@/lib/authStorage';
import { Alert } from '@/components/ui/FormField';

export function PlatformOverviewPage() {
  const canView = hasPermission('tenants.view');
  const canManage = hasPermission('tenants.manage');

  if (!canView) {
    return (
      <div>
        <PageHeader title="Platform admin" />
        <Alert tone="info">You need the tenants.view permission to access platform tools.</Alert>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Platform admin"
        description="Cross-tenant management for platform operators."
      />

      <SectionCard title="Current session">
        <dl className="grid gap-4 sm:grid-cols-2">
          <InfoItem label="Email" value={getStoredEmail() ?? '—'} />
          <InfoItem label="Role" value={getStoredRole() ?? '—'} />
          <InfoItem label="Tenants view" value={canView ? 'Allowed' : 'Denied'} />
          <InfoItem label="Tenants manage" value={canManage ? 'Allowed' : 'Denied'} />
        </dl>
      </SectionCard>

      <Link
        to="/app/platform/tenants"
        className="block rounded-2xl border border-violet-500/30 bg-violet-500/5 p-5 transition hover:border-violet-400/50 hover:bg-violet-500/10"
      >
        <h3 className="text-lg font-medium text-violet-300">Tenant directory</h3>
        <p className="mt-2 text-sm text-slate-400">
          List all workspaces, create new tenants, and enable or disable them.
        </p>
      </Link>
    </div>
  );
}

function InfoItem({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <dt className="text-xs uppercase tracking-wide text-slate-500">{label}</dt>
      <dd className="mt-1 text-sm font-medium text-slate-100">{value}</dd>
    </div>
  );
}
