import { useCallback, useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import {
  assignUserRole,
  disableUser,
  inviteUser,
  listUsers,
} from '@/lib/adminApi';
import { getApiErrorMessage } from '@/lib/errors';
import { hasPermission } from '@/lib/permissions';
import { MEMBERSHIP_ROLES, type Invitation, type TenantUser } from '@/types/admin';
import { PageHeader } from '@/components/ui/PageHeader';
import { SectionCard } from '@/components/ui/SectionCard';
import {
  Alert,
  FormField,
  PrimaryButton,
  SecondaryButton,
  SelectInput,
  TextInput,
} from '@/components/ui/FormField';

export function UsersPage() {
  const [users, setUsers] = useState<TenantUser[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [inviteEmail, setInviteEmail] = useState('');
  const [inviteRole, setInviteRole] = useState<string>('Employee');
  const [inviteError, setInviteError] = useState<string | null>(null);
  const [inviteSuccess, setInviteSuccess] = useState<Invitation | null>(null);
  const [isInviting, setIsInviting] = useState(false);
  const [actionUserId, setActionUserId] = useState<string | null>(null);

  const canView = hasPermission('users.view');
  const canInvite = hasPermission('users.invite');
  const canManage = hasPermission('users.manage');

  const loadUsers = useCallback(async () => {
    if (!canView) {
      setLoading(false);
      return;
    }

    setLoading(true);
    setError(null);

    try {
      setUsers(await listUsers());
    } catch (loadError) {
      setError(getApiErrorMessage(loadError, 'Failed to load users.'));
    } finally {
      setLoading(false);
    }
  }, [canView]);

  useEffect(() => {
    void loadUsers();
  }, [loadUsers]);

  async function handleInvite(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setInviteError(null);
    setInviteSuccess(null);
    setIsInviting(true);

    try {
      const invitation = await inviteUser(inviteEmail.trim(), inviteRole);
      setInviteSuccess(invitation);
      setInviteEmail('');
      await loadUsers();
    } catch (submitError) {
      setInviteError(getApiErrorMessage(submitError, 'Invitation failed.'));
    } finally {
      setIsInviting(false);
    }
  }

  async function handleRoleChange(userId: string, role: string) {
    setActionUserId(userId);
    try {
      await assignUserRole(userId, role);
      await loadUsers();
    } catch (actionError) {
      setError(getApiErrorMessage(actionError, 'Role update failed.'));
    } finally {
      setActionUserId(null);
    }
  }

  async function handleDisable(userId: string) {
    setActionUserId(userId);
    try {
      await disableUser(userId);
      await loadUsers();
    } catch (actionError) {
      setError(getApiErrorMessage(actionError, 'Disable failed.'));
    } finally {
      setActionUserId(null);
    }
  }

  if (!canView) {
    return (
      <div>
        <PageHeader title="Users" />
        <Alert tone="info">You need the users.view permission to access this page.</Alert>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <PageHeader title="Users" description="Manage workspace members and pending invitations." />

      {error ? <Alert tone="error">{error}</Alert> : null}

      {canInvite ? (
        <SectionCard title="Invite user">
          <form className="grid gap-4 md:grid-cols-[1fr_180px_auto]" onSubmit={handleInvite}>
            {inviteError ? <Alert tone="error">{inviteError}</Alert> : null}
            {inviteSuccess ? (
              <Alert tone="success">
                Invited {inviteSuccess.email} as {inviteSuccess.role}. Expires{' '}
                {new Date(inviteSuccess.expiresAt).toLocaleString()}.
              </Alert>
            ) : null}

            <FormField label="Email">
              <TextInput
                type="email"
                value={inviteEmail}
                onChange={(event) => setInviteEmail(event.target.value)}
                placeholder="teammate@company.com"
                required
              />
            </FormField>

            <FormField label="Role">
              <SelectInput
                value={inviteRole}
                onChange={(event) => setInviteRole(event.target.value)}
              >
                {MEMBERSHIP_ROLES.map((role) => (
                  <option key={role} value={role}>
                    {role}
                  </option>
                ))}
              </SelectInput>
            </FormField>

            <div className="flex items-end">
              <PrimaryButton type="submit" className="w-auto px-6" disabled={isInviting}>
                {isInviting ? 'Sending...' : 'Invite'}
              </PrimaryButton>
            </div>
          </form>
        </SectionCard>
      ) : null}

      <SectionCard title="Members">
        {loading ? (
          <p className="text-sm text-slate-400">Loading users...</p>
        ) : users.length === 0 ? (
          <p className="text-sm text-slate-400">No users found.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="border-b border-slate-800 text-slate-500">
                <tr>
                  <th className="px-3 py-2 font-medium">Email</th>
                  <th className="px-3 py-2 font-medium">Name</th>
                  <th className="px-3 py-2 font-medium">Role</th>
                  <th className="px-3 py-2 font-medium">Status</th>
                  <th className="px-3 py-2 font-medium">Joined</th>
                  {canManage ? <th className="px-3 py-2 font-medium">Actions</th> : null}
                </tr>
              </thead>
              <tbody>
                {users.map((user) => (
                  <tr key={user.userId} className="border-b border-slate-800/70">
                    <td className="px-3 py-3">{user.email}</td>
                    <td className="px-3 py-3 text-slate-400">{user.fullName ?? '—'}</td>
                    <td className="px-3 py-3">
                      {canManage && user.isActive ? (
                        <SelectInput
                          className="w-40"
                          value={user.role}
                          disabled={actionUserId === user.userId}
                          onChange={(event) =>
                            void handleRoleChange(user.userId, event.target.value)
                          }
                        >
                          {MEMBERSHIP_ROLES.map((role) => (
                            <option key={role} value={role}>
                              {role}
                            </option>
                          ))}
                        </SelectInput>
                      ) : (
                        user.role
                      )}
                    </td>
                    <td className="px-3 py-3">
                      <span
                        className={
                          user.isActive ? 'text-emerald-400' : 'text-rose-400'
                        }
                      >
                        {user.isActive ? 'Active' : 'Disabled'}
                      </span>
                    </td>
                    <td className="px-3 py-3 text-slate-400">
                      {new Date(user.joinedAt).toLocaleDateString()}
                    </td>
                    {canManage ? (
                      <td className="px-3 py-3">
                        {user.isActive ? (
                          <SecondaryButton
                            type="button"
                            disabled={actionUserId === user.userId}
                            onClick={() => void handleDisable(user.userId)}
                          >
                            Disable
                          </SecondaryButton>
                        ) : (
                          <span className="text-slate-500">—</span>
                        )}
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
