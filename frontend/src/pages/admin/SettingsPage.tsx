import { useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import { getCurrentTenant, updateTenantSettings } from '@/lib/adminApi';
import { getApiErrorMessage } from '@/lib/errors';
import { hasPermission } from '@/lib/permissions';
import type { Tenant } from '@/types/admin';
import { PageHeader } from '@/components/ui/PageHeader';
import { SectionCard } from '@/components/ui/SectionCard';
import { Alert, FormField, PrimaryButton, TextInput } from '@/components/ui/FormField';

export function SettingsPage() {
  const [tenant, setTenant] = useState<Tenant | null>(null);
  const [name, setName] = useState('');
  const [domain, setDomain] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [isSaving, setIsSaving] = useState(false);

  const canManage = hasPermission('settings.manage');

  useEffect(() => {
    void getCurrentTenant()
      .then((current) => {
        setTenant(current);
        setName(current.name);
        setDomain(current.domain ?? '');
      })
      .catch((loadError) => {
        setError(getApiErrorMessage(loadError, 'Failed to load tenant settings.'));
      })
      .finally(() => setLoading(false));
  }, []);

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError(null);
    setSuccess(null);
    setIsSaving(true);

    try {
      const updated = await updateTenantSettings(name.trim(), domain);
      setTenant(updated);
      setSuccess('Workspace settings updated.');
    } catch (submitError) {
      setError(getApiErrorMessage(submitError, 'Failed to update settings.'));
    } finally {
      setIsSaving(false);
    }
  }

  if (!canManage) {
    return (
      <div>
        <PageHeader title="Settings" />
        <Alert tone="info">You need the settings.manage permission to access this page.</Alert>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Settings"
        description="Update workspace profile details for the current tenant."
      />

      {error ? <Alert tone="error">{error}</Alert> : null}
      {success ? <Alert tone="success">{success}</Alert> : null}

      <SectionCard title="Workspace profile">
        {loading ? (
          <p className="text-sm text-slate-400">Loading settings...</p>
        ) : (
          <form className="max-w-xl space-y-4" onSubmit={handleSubmit}>
            <FormField label="Slug" hint="Read-only identifier used in login and API headers.">
              <TextInput value={tenant?.slug ?? ''} disabled />
            </FormField>

            <FormField label="Workspace name">
              <TextInput
                value={name}
                onChange={(event) => setName(event.target.value)}
                required
              />
            </FormField>

            <FormField label="Custom domain" hint="Optional. Leave blank to clear.">
              <TextInput
                value={domain}
                onChange={(event) => setDomain(event.target.value)}
                placeholder="acme.example.com"
              />
            </FormField>

            <PrimaryButton type="submit" className="w-auto px-6" disabled={isSaving}>
              {isSaving ? 'Saving...' : 'Save settings'}
            </PrimaryButton>
          </form>
        )}
      </SectionCard>
    </div>
  );
}
