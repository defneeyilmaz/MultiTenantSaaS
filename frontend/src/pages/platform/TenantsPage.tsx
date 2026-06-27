import { useCallback, useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import {
  createPlatformTenant,
  listPlatformTenants,
  updatePlatformTenant,
} from '@/lib/platformApi';
import { getApiErrorMessage } from '@/lib/errors';
import { hasPermission } from '@/lib/permissions';
import type { PlatformTenant } from '@/types/platform';
import { PageHeader } from '@/components/ui/PageHeader';
import { SectionCard } from '@/components/ui/SectionCard';
import {
  Alert,
  FormField,
  PrimaryButton,
  SecondaryButton,
  TextInput,
} from '@/components/ui/FormField';

export function TenantsPage() {
  const [tenants, setTenants] = useState<PlatformTenant[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [name, setName] = useState('');
  const [slug, setSlug] = useState('');
  const [domain, setDomain] = useState('');
  const [isCreating, setIsCreating] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [editName, setEditName] = useState('');
  const [editDomain, setEditDomain] = useState('');
  const [actionTenantId, setActionTenantId] = useState<string | null>(null);

  const canView = hasPermission('tenants.view');
  const canManage = hasPermission('tenants.manage');

  const loadTenants = useCallback(async () => {
    if (!canView) {
      setLoading(false);
      return;
    }

    setLoading(true);
    setError(null);

    try {
      setTenants(await listPlatformTenants());
    } catch (loadError) {
      setError(getApiErrorMessage(loadError, 'Failed to load tenants.'));
    } finally {
      setLoading(false);
    }
  }, [canView]);

  useEffect(() => {
    void loadTenants();
  }, [loadTenants]);

  async function handleCreate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsCreating(true);
    setError(null);

    try {
      await createPlatformTenant({ name, slug, domain });
      setName('');
      setSlug('');
      setDomain('');
      await loadTenants();
    } catch (submitError) {
      setError(getApiErrorMessage(submitError, 'Failed to create tenant.'));
    } finally {
      setIsCreating(false);
    }
  }

  function startEdit(tenant: PlatformTenant) {
    setEditingId(tenant.id);
    setEditName(tenant.name);
    setEditDomain(tenant.domain ?? '');
  }

  function cancelEdit() {
    setEditingId(null);
    setEditName('');
    setEditDomain('');
  }

  async function handleSaveEdit(tenantId: string) {
    setActionTenantId(tenantId);
    setError(null);

    try {
      await updatePlatformTenant(tenantId, {
        name: editName.trim(),
        domain: editDomain,
      });
      cancelEdit();
      await loadTenants();
    } catch (submitError) {
      setError(getApiErrorMessage(submitError, 'Failed to update tenant.'));
    } finally {
      setActionTenantId(null);
    }
  }

  async function handleToggleActive(tenant: PlatformTenant) {
    setActionTenantId(tenant.id);
    setError(null);

    try {
      await updatePlatformTenant(tenant.id, { isActive: !tenant.isActive });
      await loadTenants();
    } catch (submitError) {
      setError(getApiErrorMessage(submitError, 'Failed to update tenant status.'));
    } finally {
      setActionTenantId(null);
    }
  }

  if (!canView) {
    return (
      <div>
        <PageHeader title="Tenants" />
        <Alert tone="info">You need the tenants.view permission to access this page.</Alert>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Tenants"
        description="All workspaces registered on the platform."
      />

      {error ? <Alert tone="error">{error}</Alert> : null}

      {canManage ? (
        <SectionCard title="Create tenant">
          <form className="grid gap-4 md:grid-cols-3" onSubmit={handleCreate}>
            <FormField label="Name">
              <TextInput
                value={name}
                onChange={(event) => setName(event.target.value)}
                placeholder="Initech"
                required
              />
            </FormField>
            <FormField label="Slug" hint="Optional. Generated from name if empty.">
              <TextInput
                value={slug}
                onChange={(event) => setSlug(event.target.value)}
                placeholder="initech"
              />
            </FormField>
            <FormField label="Domain">
              <TextInput
                value={domain}
                onChange={(event) => setDomain(event.target.value)}
                placeholder="initech.example.com"
              />
            </FormField>
            <div className="md:col-span-3">
              <PrimaryButton type="submit" className="w-auto px-6" disabled={isCreating}>
                {isCreating ? 'Creating...' : 'Create tenant'}
              </PrimaryButton>
            </div>
          </form>
        </SectionCard>
      ) : null}

      <SectionCard title="Tenant directory">
        {loading ? (
          <p className="text-sm text-slate-400">Loading tenants...</p>
        ) : tenants.length === 0 ? (
          <p className="text-sm text-slate-400">No tenants found.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="border-b border-slate-800 text-slate-500">
                <tr>
                  <th className="px-3 py-2 font-medium">Name</th>
                  <th className="px-3 py-2 font-medium">Slug</th>
                  <th className="px-3 py-2 font-medium">Domain</th>
                  <th className="px-3 py-2 font-medium">Status</th>
                  <th className="px-3 py-2 font-medium">Created</th>
                  {canManage ? <th className="px-3 py-2 font-medium">Actions</th> : null}
                </tr>
              </thead>
              <tbody>
                {tenants.map((tenant) => (
                  <tr key={tenant.id} className="border-b border-slate-800/70 align-top">
                    <td className="px-3 py-3">
                      {editingId === tenant.id ? (
                        <TextInput
                          value={editName}
                          onChange={(event) => setEditName(event.target.value)}
                        />
                      ) : (
                        tenant.name
                      )}
                    </td>
                    <td className="px-3 py-3 font-mono text-slate-400">{tenant.slug}</td>
                    <td className="px-3 py-3">
                      {editingId === tenant.id ? (
                        <TextInput
                          value={editDomain}
                          onChange={(event) => setEditDomain(event.target.value)}
                          placeholder="optional"
                        />
                      ) : (
                        <span className="text-slate-400">{tenant.domain ?? '—'}</span>
                      )}
                    </td>
                    <td className="px-3 py-3">
                      <span
                        className={tenant.isActive ? 'text-emerald-400' : 'text-rose-400'}
                      >
                        {tenant.isActive ? 'Active' : 'Disabled'}
                      </span>
                    </td>
                    <td className="px-3 py-3 text-slate-400">
                      {new Date(tenant.createdAt).toLocaleDateString()}
                    </td>
                    {canManage ? (
                      <td className="px-3 py-3">
                        <div className="flex flex-wrap gap-2">
                          {editingId === tenant.id ? (
                            <>
                              <SecondaryButton
                                type="button"
                                disabled={actionTenantId === tenant.id}
                                onClick={() => void handleSaveEdit(tenant.id)}
                              >
                                Save
                              </SecondaryButton>
                              <SecondaryButton type="button" onClick={cancelEdit}>
                                Cancel
                              </SecondaryButton>
                            </>
                          ) : (
                            <>
                              <SecondaryButton type="button" onClick={() => startEdit(tenant)}>
                                Edit
                              </SecondaryButton>
                              <SecondaryButton
                                type="button"
                                disabled={actionTenantId === tenant.id}
                                onClick={() => void handleToggleActive(tenant)}
                              >
                                {tenant.isActive ? 'Disable' : 'Enable'}
                              </SecondaryButton>
                            </>
                          )}
                        </div>
                      </td>
                    ) : null}
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </SectionCard>
    </div>
  );
}
