import { useCallback, useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import {
  createRole,
  listPermissions,
  listRoles,
  updateRolePermissions,
} from '@/lib/adminApi';
import { getApiErrorMessage } from '@/lib/errors';
import { hasPermission } from '@/lib/permissions';
import type { Permission, Role } from '@/types/admin';
import { PageHeader } from '@/components/ui/PageHeader';
import { SectionCard } from '@/components/ui/SectionCard';
import {
  Alert,
  FormField,
  PrimaryButton,
  TextInput,
} from '@/components/ui/FormField';

export function RolesPage() {
  const [roles, setRoles] = useState<Role[]>([]);
  const [permissions, setPermissions] = useState<Permission[]>([]);
  const [selectedRoleId, setSelectedRoleId] = useState<string | null>(null);
  const [selectedPermissions, setSelectedPermissions] = useState<string[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [createName, setCreateName] = useState('');
  const [createDescription, setCreateDescription] = useState('');
  const [isCreating, setIsCreating] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  const canView = hasPermission('roles.view');
  const canManage = hasPermission('roles.manage');
  const canViewPermissions = hasPermission('permissions.view');

  const loadData = useCallback(async () => {
    if (!canView) {
      setLoading(false);
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const roleList = await listRoles();
      setRoles(roleList);

      if (canViewPermissions) {
        setPermissions(await listPermissions());
      }

      if (roleList.length > 0 && !selectedRoleId) {
        setSelectedRoleId(roleList[0].id);
        setSelectedPermissions([...roleList[0].permissions]);
      }
    } catch (loadError) {
      setError(getApiErrorMessage(loadError, 'Failed to load roles.'));
    } finally {
      setLoading(false);
    }
  }, [canView, canViewPermissions]);

  useEffect(() => {
    void loadData();
  }, [loadData]);

  function selectRole(role: Role) {
    setSelectedRoleId(role.id);
    setSelectedPermissions([...role.permissions]);
  }

  function togglePermission(name: string) {
    setSelectedPermissions((current) =>
      current.includes(name) ? current.filter((item) => item !== name) : [...current, name],
    );
  }

  async function handleCreateRole(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsCreating(true);
    setError(null);

    try {
      const role = await createRole(createName.trim(), createDescription.trim() || undefined);
      setCreateName('');
      setCreateDescription('');
      setRoles((current) => [...current, role]);
      selectRole(role);
    } catch (submitError) {
      setError(getApiErrorMessage(submitError, 'Failed to create role.'));
    } finally {
      setIsCreating(false);
    }
  }

  async function handleSavePermissions() {
    if (!selectedRoleId) {
      return;
    }

    setIsSaving(true);
    setError(null);

    try {
      const updated = await updateRolePermissions(selectedRoleId, selectedPermissions);
      setRoles((current) => current.map((role) => (role.id === updated.id ? updated : role)));
      setSelectedPermissions([...updated.permissions]);
    } catch (submitError) {
      setError(getApiErrorMessage(submitError, 'Failed to update permissions.'));
    } finally {
      setIsSaving(false);
    }
  }

  if (!canView) {
    return (
      <div>
        <PageHeader title="Roles" />
        <Alert tone="info">You need the roles.view permission to access this page.</Alert>
      </div>
    );
  }

  const selectedRole = roles.find((role) => role.id === selectedRoleId) ?? null;
  const isPlatformAdmin = selectedRole?.name === 'PlatformAdmin';

  return (
    <div className="space-y-6">
      <PageHeader
        title="Roles"
        description="Global role templates and their permission mappings."
      />

      {error ? <Alert tone="error">{error}</Alert> : null}

      {canManage ? (
        <SectionCard title="Create role">
          <form className="grid gap-4 md:grid-cols-2" onSubmit={handleCreateRole}>
            <FormField label="Name">
              <TextInput
                value={createName}
                onChange={(event) => setCreateName(event.target.value)}
                placeholder="Support"
                required
              />
            </FormField>
            <FormField label="Description">
              <TextInput
                value={createDescription}
                onChange={(event) => setCreateDescription(event.target.value)}
                placeholder="Support team access"
              />
            </FormField>
            <div className="md:col-span-2">
              <PrimaryButton type="submit" className="w-auto px-6" disabled={isCreating}>
                {isCreating ? 'Creating...' : 'Create role'}
              </PrimaryButton>
            </div>
          </form>
        </SectionCard>
      ) : null}

      <div className="grid gap-6 lg:grid-cols-[280px_1fr]">
        <SectionCard title="Roles">
          {loading ? (
            <p className="text-sm text-slate-400">Loading...</p>
          ) : (
            <ul className="space-y-1">
              {roles.map((role) => (
                <li key={role.id}>
                  <button
                    type="button"
                    onClick={() => selectRole(role)}
                    className={[
                      'w-full rounded-lg px-3 py-2 text-left text-sm transition',
                      role.id === selectedRoleId
                        ? 'bg-sky-500/15 text-sky-300'
                        : 'text-slate-300 hover:bg-slate-800',
                    ].join(' ')}
                  >
                    <span className="font-medium">{role.name}</span>
                    {role.description ? (
                      <span className="mt-0.5 block text-xs text-slate-500">
                        {role.description}
                      </span>
                    ) : null}
                  </button>
                </li>
              ))}
            </ul>
          )}
        </SectionCard>

        <SectionCard
          title={selectedRole ? `${selectedRole.name} permissions` : 'Permissions'}
          description={
            selectedRole
              ? `${selectedRole.permissions.length} permission(s) assigned`
              : undefined
          }
        >
          {!selectedRole ? (
            <p className="text-sm text-slate-400">Select a role to view permissions.</p>
          ) : !canViewPermissions ? (
            <ul className="space-y-2 text-sm text-slate-300">
              {selectedRole.permissions.map((permission) => (
                <li key={permission} className="rounded-lg bg-slate-950/60 px-3 py-2">
                  {permission}
                </li>
              ))}
            </ul>
          ) : isPlatformAdmin ? (
            <Alert tone="info">PlatformAdmin permissions cannot be modified.</Alert>
          ) : (
            <>
              <div className="grid gap-2 sm:grid-cols-2">
                {permissions.map((permission) => (
                  <label
                    key={permission.id}
                    className="flex cursor-pointer items-start gap-3 rounded-lg border border-slate-800 bg-slate-950/40 px-3 py-2"
                  >
                    <input
                      type="checkbox"
                      className="mt-1"
                      checked={selectedPermissions.includes(permission.name)}
                      disabled={!canManage}
                      onChange={() => togglePermission(permission.name)}
                    />
                    <span>
                      <span className="block text-sm font-medium">{permission.name}</span>
                      {permission.description ? (
                        <span className="block text-xs text-slate-500">
                          {permission.description}
                        </span>
                      ) : null}
                    </span>
                  </label>
                ))}
              </div>

              {canManage ? (
                <div className="mt-4">
                  <PrimaryButton
                    type="button"
                    className="w-auto px-6"
                    disabled={isSaving}
                    onClick={() => void handleSavePermissions()}
                  >
                    {isSaving ? 'Saving...' : 'Save permissions'}
                  </PrimaryButton>
                </div>
              ) : null}
            </>
          )}
        </SectionCard>
      </div>
    </div>
  );
}
