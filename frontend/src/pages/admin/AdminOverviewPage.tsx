import { Link } from 'react-router-dom';
import { PageHeader } from '@/components/ui/PageHeader';
import { SectionCard } from '@/components/ui/SectionCard';
import { getCurrentTenant } from '@/lib/adminApi';
import { hasPermission } from '@/lib/permissions';
import { getStoredEmail, getStoredRole, getStoredTenantSlug } from '@/lib/authStorage';
import { useEffect, useState } from 'react';
import type { Tenant } from '@/types/admin';

const sections = [
  {
    title: 'Users',
    description: 'Invite teammates, assign roles, and disable accounts.',
    to: '/app/admin/users',
    permission: 'users.view',
  },
  {
    title: 'Roles',
    description: 'Review role templates and permission mappings.',
    to: '/app/admin/roles',
    permission: 'roles.view',
  },
  {
    title: 'Audit log',
    description: 'Track sign-ins, invitations, and settings changes.',
    to: '/app/admin/audit',
    permission: 'audit.view',
  },
  {
    title: 'Settings',
    description: 'Update workspace name and custom domain.',
    to: '/app/admin/settings',
    permission: 'settings.manage',
  },
];

export function AdminOverviewPage() {
  const [tenant, setTenant] = useState<Tenant | null>(null);

  useEffect(() => {
    void getCurrentTenant().then(setTenant).catch(() => setTenant(null));
  }, []);

  const visibleSections = sections.filter((section) => hasPermission(section.permission));

  return (
    <div className="space-y-6">
      <PageHeader
        title="Tenant admin"
        description="Manage users, roles, audit history, and workspace settings."
      />

      <SectionCard title="Current session">
        <dl className="grid gap-4 sm:grid-cols-2">
          <InfoItem label="Email" value={getStoredEmail() ?? '—'} />
          <InfoItem label="Role" value={getStoredRole() ?? '—'} />
          <InfoItem label="Tenant slug" value={getStoredTenantSlug() ?? '—'} />
          <InfoItem label="Workspace" value={tenant?.name ?? 'Loading...'} />
        </dl>
      </SectionCard>

      {visibleSections.length > 0 ? (
        <div className="grid gap-4 md:grid-cols-2">
          {visibleSections.map((section) => (
            <Link
              key={section.to}
              to={section.to}
              className="rounded-2xl border border-slate-800 bg-slate-900/40 p-5 transition hover:border-sky-500/40 hover:bg-slate-900/70"
            >
              <h3 className="text-lg font-medium text-sky-300">{section.title}</h3>
              <p className="mt-2 text-sm text-slate-400">{section.description}</p>
            </Link>
          ))}
        </div>
      ) : (
        <SectionCard title="Limited access">
          <p className="text-sm text-slate-400">
            Your role does not include tenant administration permissions. Contact a workspace
            admin if you need access.
          </p>
        </SectionCard>
      )}
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
