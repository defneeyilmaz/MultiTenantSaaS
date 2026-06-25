import { useCallback, useEffect, useState } from 'react';
import type { FormEvent } from 'react';
import { listUsers } from '@/lib/adminApi';
import { createTask, listProjects, listTasks, updateTaskStatus } from '@/lib/workspaceApi';
import { getApiErrorMessage } from '@/lib/errors';
import { hasPermission } from '@/lib/permissions';
import type { TenantUser } from '@/types/admin';
import {
  TASK_STATUSES,
  normalizeTaskStatus,
  taskStatusLabel,
  type Project,
  type TaskItem,
} from '@/types/workspace';
import { PageHeader } from '@/components/ui/PageHeader';
import { SectionCard } from '@/components/ui/SectionCard';
import {
  Alert,
  FormField,
  PrimaryButton,
  SelectInput,
  TextInput,
} from '@/components/ui/FormField';

export function TasksPage() {
  const [projects, setProjects] = useState<Project[]>([]);
  const [tasks, setTasks] = useState<TaskItem[]>([]);
  const [users, setUsers] = useState<TenantUser[]>([]);
  const [projectFilter, setProjectFilter] = useState('');
  const [projectId, setProjectId] = useState('');
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [assignedToUserId, setAssignedToUserId] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isCreating, setIsCreating] = useState(false);
  const [updatingTaskId, setUpdatingTaskId] = useState<string | null>(null);

  const canView = hasPermission('tasks.view');
  const canCreate = hasPermission('tasks.create');
  const canUpdateStatus = hasPermission('tasks.update_status');
  const canListUsers = hasPermission('users.view');

  const loadTasks = useCallback(async () => {
    if (!canView) {
      setLoading(false);
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const [projectList, taskList] = await Promise.all([
        listProjects(),
        listTasks(projectFilter || undefined),
      ]);

      setProjects(projectList);
      setTasks(taskList);

      if (!projectId && projectList.length > 0) {
        setProjectId(projectList[0].id);
      }

      if (canListUsers) {
        setUsers(await listUsers());
      }
    } catch (loadError) {
      setError(getApiErrorMessage(loadError, 'Failed to load tasks.'));
    } finally {
      setLoading(false);
    }
  }, [canView, canListUsers, projectFilter]);

  useEffect(() => {
    void loadTasks();
  }, [loadTasks]);

  async function handleCreate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsCreating(true);
    setError(null);

    try {
      await createTask({
        projectId,
        title: title.trim(),
        description,
        assignedToUserId: assignedToUserId || undefined,
      });
      setTitle('');
      setDescription('');
      setAssignedToUserId('');
      await loadTasks();
    } catch (submitError) {
      setError(getApiErrorMessage(submitError, 'Failed to create task.'));
    } finally {
      setIsCreating(false);
    }
  }

  async function handleStatusChange(taskId: string, status: number) {
    setUpdatingTaskId(taskId);
    setError(null);

    try {
      await updateTaskStatus(taskId, status);
      await loadTasks();
    } catch (submitError) {
      setError(getApiErrorMessage(submitError, 'Failed to update task status.'));
    } finally {
      setUpdatingTaskId(null);
    }
  }

  function projectName(id: string): string {
    return projects.find((project) => project.id === id)?.name ?? id;
  }

  function assigneeLabel(userId: string | null): string {
    if (!userId) {
      return 'Unassigned';
    }

    const user = users.find((item) => item.userId === userId);
    return user?.email ?? userId;
  }

  if (!canView) {
    return (
      <div>
        <PageHeader title="Tasks" />
        <Alert tone="info">You need the tasks.view permission to access this page.</Alert>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <PageHeader title="Tasks" description="Work items scoped to tenant projects." />

      {error ? <Alert tone="error">{error}</Alert> : null}

      <SectionCard title="Filter">
        <FormField label="Project">
          <SelectInput
            value={projectFilter}
            onChange={(event) => setProjectFilter(event.target.value)}
          >
            <option value="">All projects</option>
            {projects.map((project) => (
              <option key={project.id} value={project.id}>
                {project.name}
              </option>
            ))}
          </SelectInput>
        </FormField>
      </SectionCard>

      {canCreate ? (
        <SectionCard title="Create task">
          <form className="grid gap-4 md:grid-cols-2" onSubmit={handleCreate}>
            <FormField label="Project">
              <SelectInput
                value={projectId}
                onChange={(event) => setProjectId(event.target.value)}
                required
              >
                {projects.map((project) => (
                  <option key={project.id} value={project.id}>
                    {project.name}
                  </option>
                ))}
              </SelectInput>
            </FormField>

            <FormField label="Title">
              <TextInput
                value={title}
                onChange={(event) => setTitle(event.target.value)}
                placeholder="Draft release notes"
                required
              />
            </FormField>

            <FormField label="Description">
              <TextInput
                value={description}
                onChange={(event) => setDescription(event.target.value)}
                placeholder="Optional details"
              />
            </FormField>

            {canListUsers ? (
              <FormField label="Assignee">
                <SelectInput
                  value={assignedToUserId}
                  onChange={(event) => setAssignedToUserId(event.target.value)}
                >
                  <option value="">Unassigned</option>
                  {users
                    .filter((user) => user.isActive)
                    .map((user) => (
                      <option key={user.userId} value={user.userId}>
                        {user.email}
                      </option>
                    ))}
                </SelectInput>
              </FormField>
            ) : null}

            <div className="md:col-span-2">
              <PrimaryButton
                type="submit"
                className="w-auto px-6"
                disabled={isCreating || projects.length === 0}
              >
                {isCreating ? 'Creating...' : 'Create task'}
              </PrimaryButton>
            </div>
          </form>
        </SectionCard>
      ) : null}

      <SectionCard title="Task board">
        {loading ? (
          <p className="text-sm text-slate-400">Loading tasks...</p>
        ) : tasks.length === 0 ? (
          <p className="text-sm text-slate-400">No tasks found.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full text-left text-sm">
              <thead className="border-b border-slate-800 text-slate-500">
                <tr>
                  <th className="px-3 py-2 font-medium">Title</th>
                  <th className="px-3 py-2 font-medium">Project</th>
                  <th className="px-3 py-2 font-medium">Status</th>
                  <th className="px-3 py-2 font-medium">Assignee</th>
                  <th className="px-3 py-2 font-medium">Created</th>
                </tr>
              </thead>
              <tbody>
                {tasks.map((task) => (
                  <tr key={task.id} className="border-b border-slate-800/70 align-top">
                    <td className="px-3 py-3">
                      <p className="font-medium text-slate-100">{task.title}</p>
                      {task.description ? (
                        <p className="mt-1 text-xs text-slate-500">{task.description}</p>
                      ) : null}
                    </td>
                    <td className="px-3 py-3 text-slate-400">{projectName(task.projectId)}</td>
                    <td className="px-3 py-3">
                      {canUpdateStatus ? (
                        <SelectInput
                          className="w-36"
                          value={normalizeTaskStatus(task.status)}
                          disabled={updatingTaskId === task.id}
                          onChange={(event) =>
                            void handleStatusChange(task.id, Number(event.target.value))
                          }
                        >
                          {TASK_STATUSES.map((status) => (
                            <option key={status.value} value={status.value}>
                              {status.label}
                            </option>
                          ))}
                        </SelectInput>
                      ) : (
                        taskStatusLabel(task.status)
                      )}
                    </td>
                    <td className="px-3 py-3 text-slate-400">
                      {assigneeLabel(task.assignedToUserId)}
                    </td>
                    <td className="px-3 py-3 text-slate-400">
                      {new Date(task.createdAt).toLocaleDateString()}
                    </td>
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
