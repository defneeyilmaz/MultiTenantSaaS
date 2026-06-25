import { useEffect, useState } from 'react';
import { getCurrentTenant } from '@/lib/adminApi';
import { getApiErrorMessage } from '@/lib/errors';
import { getStoredPermissions } from '@/lib/permissions';
import {
  getStoredEmail,
  getStoredRole,
  getStoredTenantId,
  getStoredTenantSlug,
  getStoredUserId,
} from '@/lib/authStorage';
import type { Tenant } from '@/types/admin';
import { PageHeader } from '@/components/ui/PageHeader';
import { SectionCard } from '@/components/ui/SectionCard';
import { Alert } from '@/components/ui/FormField';

export function ProfilePage() {
  const [tenant, setTenant] = useState<Tenant | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    void getCurrentTenant()
      .then(setTenant)
      .catch((loadError) => {
        setError(getApiErrorMessage(loadError, 'Failed to load workspace profile.'));
      });
  }, []);

  const permissions = getStoredPermissions();

  return (
    <div className="space-y-6">
      <PageHeader
        title="Profile"
        description="Your session details and current workspace context."
      />

      {error ? <Alert tone="error">{error}</Alert> : null}

      <SectionCard title="Account">
        <dl className="grid gap-4 sm:grid-cols-2">
          <InfoItem label="Email" value={getStoredEmail() ?? '—'} />
          <InfoItem label="Role" value={getStoredRole() ?? '—'} />
          <InfoItem label="User ID" value={getStoredUserId() ?? '—'} />
          <InfoItem label="Tenant ID" value={getStoredTenantId() ?? '—'} />
        </dl>
      </SectionCard>

      <SectionCard title="Workspace">
        <dl className="grid gap-4 sm:grid-cols-2">
          <InfoItem label="Slug" value={getStoredTenantSlug() ?? '—'} />
          <InfoItem label="Name" value={tenant?.name ?? 'Loading...'} />
          <InfoItem label="Domain" value={tenant?.domain ?? '—'} />
          <InfoItem
            label="Status"
            value={tenant ? (tenant.isActive ? 'Active' : 'Inactive') : '—'}
          />
        </dl>
      </SectionCard>

      <SectionCard title="Permissions" description="From your current access token.">
        {permissions.length === 0 ? (
          <p className="text-sm text-slate-400">No permissions found in token.</p>
        ) : (
          <ul className="grid gap-2 sm:grid-cols-2">
            {permissions.map((permission) => (
              <li
                key={permission}
                className="rounded-lg border border-slate-800 bg-slate-950/50 px-3 py-2 text-sm text-slate-300"
              >
                {permission}
              </li>
            ))}
          </ul>
        )}
      </SectionCard>
    </div>
  );
}

function InfoItem({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <dt className="text-xs uppercase tracking-wide text-slate-500">{label}</dt>
      <dd className="mt-1 text-sm font-medium text-slate-100 break-all">{value}</dd>
    </div>
  );
}
