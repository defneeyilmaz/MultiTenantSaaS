import { Link } from 'react-router-dom';
import { PageHeader } from '@/components/ui/PageHeader';
import { SectionCard } from '@/components/ui/SectionCard';
import { hasPermission } from '@/lib/permissions';
import {
  getStoredEmail,
  getStoredRole,
  getStoredTenantSlug,
  getStoredUserId,
} from '@/lib/authStorage';

const sections = [
  {
    title: 'Projects',
    description: 'Browse and create tenant projects.',
    to: '/app/workspace/projects',
    permission: 'projects.view',
  },
  {
    title: 'Tasks',
    description: 'Track work items and update status.',
    to: '/app/workspace/tasks',
    permission: 'tasks.view',
  },
  {
    title: 'Profile',
    description: 'Review your session and workspace context.',
    to: '/app/workspace/profile',
    permission: null,
  },
];

export function WorkspaceOverviewPage() {
  const visibleSections = sections.filter(
    (section) => !section.permission || hasPermission(section.permission),
  );

  return (
    <div className="space-y-6">
      <PageHeader
        title="Workspace"
        description="Projects, tasks, and your profile for the current tenant."
      />

      <SectionCard title="Signed in as">
        <dl className="grid gap-4 sm:grid-cols-2">
          <InfoItem label="Email" value={getStoredEmail() ?? '—'} />
          <InfoItem label="Role" value={getStoredRole() ?? '—'} />
          <InfoItem label="Tenant" value={getStoredTenantSlug() ?? '—'} />
          <InfoItem label="User ID" value={getStoredUserId() ?? '—'} />
        </dl>
      </SectionCard>

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
