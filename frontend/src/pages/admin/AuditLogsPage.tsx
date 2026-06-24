import { useCallback, useEffect, useState } from 'react';
import { listAuditLogs } from '@/lib/adminApi';
import { getApiErrorMessage } from '@/lib/errors';
import { hasPermission } from '@/lib/permissions';
import type { AuditLog } from '@/types/admin';
import { PageHeader } from '@/components/ui/PageHeader';
import { SectionCard } from '@/components/ui/SectionCard';
import { Alert } from '@/components/ui/FormField';

export function AuditLogsPage() {
  const [logs, setLogs] = useState<AuditLog[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const canView = hasPermission('audit.view');

  const loadLogs = useCallback(async () => {
    if (!canView) {
      setLoading(false);
      return;
    }

    setLoading(true);
    setError(null);

    try {
      setLogs(await listAuditLogs());
    } catch (loadError) {
      setError(getApiErrorMessage(loadError, 'Failed to load audit logs.'));
    } finally {
      setLoading(false);
    }
  }, [canView]);

  useEffect(() => {
    void loadLogs();
  }, [loadLogs]);

  if (!canView) {
    return (
      <div>
        <PageHeader title="Audit log" />
        <Alert tone="info">You need the audit.view permission to access this page.</Alert>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <PageHeader
        title="Audit log"
        description="Recent workspace activity (latest 100 events)."
      />

      {error ? <Alert tone="error">{error}</Alert> : null}

      <SectionCard>
        {loading ? (
          <p className="text-sm text-slate-400">Loading audit logs...</p>
        ) : logs.length === 0 ? (
          <p className="text-sm text-slate-400">No audit events yet.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="border-b border-slate-800 text-slate-500">
                <tr>
                  <th className="px-3 py-2 font-medium">Time</th>
                  <th className="px-3 py-2 font-medium">Action</th>
                  <th className="px-3 py-2 font-medium">Actor</th>
                  <th className="px-3 py-2 font-medium">Entity</th>
                  <th className="px-3 py-2 font-medium">Details</th>
                  <th className="px-3 py-2 font-medium">IP</th>
                </tr>
              </thead>
              <tbody>
                {logs.map((log) => (
                  <tr key={log.id} className="border-b border-slate-800/70 align-top">
                    <td className="px-3 py-3 whitespace-nowrap text-slate-400">
                      {new Date(log.createdAt).toLocaleString()}
                    </td>
                    <td className="px-3 py-3 font-medium text-sky-300">{log.action}</td>
                    <td className="px-3 py-3">{log.actorEmail ?? '—'}</td>
                    <td className="px-3 py-3 text-slate-400">
                      {log.entityType ?? '—'}
                      {log.entityId ? (
                        <span className="mt-1 block text-xs text-slate-600">{log.entityId}</span>
                      ) : null}
                    </td>
                    <td className="px-3 py-3 max-w-xs text-slate-300">{log.details ?? '—'}</td>
                    <td className="px-3 py-3 text-slate-400">{log.ipAddress ?? '—'}</td>
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
